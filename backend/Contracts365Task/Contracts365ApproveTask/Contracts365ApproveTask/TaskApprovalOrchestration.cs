using Contracts365ApproveTask.Constants;
using Contracts365ApproveTask.Dtos;
using Contracts365ApproveTask.Exceptions;
using Contracts365ApproveTask.Helpers;
using Contracts365ApproveTask.Interfaces;
using Contracts365ApproveTask.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Contracts365ApproveTask;

/// <summary>
/// Orchestrates the task approval workflow using Azure Durable Functions.
/// This class manages the lifecycle of approval processes including initialization,
/// approval/rejection actions, and email notifications at each stage.
/// </summary>
public class TaskApprovalOrchestration
{
    private readonly ILogger<TaskApprovalOrchestration> _logger;
    private readonly IEmailService _emailService;
    private readonly IHttpRequestReader _requestReader;
    private readonly IHttpResponseWriter _responseWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskApprovalOrchestration"/> class.
    /// </summary>
    /// <param name="logger">Logger for telemetry and diagnostics.</param>
    /// <param name="emailService">Service for sending email notifications.</param>
    /// <param name="requestReader">Service for reading and deserializing HTTP requests.</param>
    /// <param name="responseWriter">Service for creating and writing HTTP responses.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
    public TaskApprovalOrchestration(
        ILogger<TaskApprovalOrchestration> logger,
        IEmailService emailService,
        IHttpRequestReader requestReader,
        IHttpResponseWriter responseWriter)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _requestReader = requestReader ?? throw new ArgumentNullException(nameof(requestReader));
        _responseWriter = responseWriter ?? throw new ArgumentNullException(nameof(responseWriter));
    }

    /// <summary>
    /// The main orchestrator function that coordinates the approval workflow.
    /// </summary>
    /// <param name="context">The orchestration context that provides access to orchestration capabilities.</param>
    /// <returns>The final approval status: "Approve" or "Reject".</returns>
    /// <exception cref="InvalidInputException">Thrown when the email is empty or null.</exception>
    /// <exception cref="EmailValidationException">Thrown when the email format is invalid.</exception>
    /// <exception cref="InvalidApprovalEventException">Thrown when the approval event value is not recognized.</exception>
    [Function(TaskApprovalConstants.FunctionNames.TaskApprovalOrchestration)]
    public async Task<string> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        // Get the user email from the orchestration input
        var userEmail = context.GetInput<string>();
        if (string.IsNullOrEmpty(userEmail))
        {
            throw new InvalidInputException(TaskApprovalConstants.Errors.EmptyEmail);
        }
        if (!userEmail.IsValidEmail())
        {
            throw new EmailValidationException(userEmail);
        }

        // Send initial notification that approval process has started
        await context.CallActivityAsync(TaskApprovalConstants.FunctionNames.SendEmail, new EmailRequest
        {
            Email = userEmail,
            Subject = TaskApprovalConstants.EmailTemplates.Subjects.TaskApprovalStarted,
            Content = TaskApprovalConstants.EmailTemplates.Contents.TaskApprovalStarted
        });

        // Wait for an external approval/rejection event
        var approvalEvent = await context.WaitForExternalEvent<string>(TaskApprovalConstants.Events.ApprovalEvent);

        // Process the approval event
        if (approvalEvent.Equals(TaskApprovalConstants.Actions.Approve, StringComparison.OrdinalIgnoreCase))
        {
            // Send approval confirmation email
            await context.CallActivityAsync(TaskApprovalConstants.FunctionNames.SendEmail, new EmailRequest
            {
                Email = userEmail,
                Subject = TaskApprovalConstants.EmailTemplates.Subjects.TaskApproved,
                Content = TaskApprovalConstants.EmailTemplates.Contents.TaskApproved
            });
            return TaskApprovalConstants.Actions.Approve;
        }
        else if (approvalEvent.Equals(TaskApprovalConstants.Actions.Reject, StringComparison.OrdinalIgnoreCase))
        {
            // Send rejection confirmation email
            await context.CallActivityAsync(TaskApprovalConstants.FunctionNames.SendEmail, new EmailRequest
            {
                Email = userEmail,
                Subject = TaskApprovalConstants.EmailTemplates.Subjects.TaskRejected,
                Content = TaskApprovalConstants.EmailTemplates.Contents.TaskRejected
            });
            return TaskApprovalConstants.Actions.Reject;
        }
        else
        {
            // Unrecognized approval event
            throw new InvalidApprovalEventException(approvalEvent);
        }
    }

    /// <summary>
    /// HTTP trigger function that initiates a new task approval workflow.
    /// </summary>
    /// <param name="req">The HTTP request containing the approval request with user email.</param>
    /// <param name="client">The durable task client used to schedule the orchestration.</param>
    /// <returns>HTTP response with the instance ID, user email, and status.</returns>
    /// <exception cref="InvalidInputException">Thrown when the user email is empty or null.</exception>
    /// <exception cref="EmailValidationException">Thrown when the email format is invalid.</exception>
    [Function("StartApproval")]
    public async Task<HttpResponseData> StartApproval(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        // Deserialize the request body
        var content = await _requestReader.ReadFromJsonAsync<ApprovalRequest>(req);
        if (content == null || string.IsNullOrEmpty(content.UserEmail))
        {
            throw new InvalidInputException(TaskApprovalConstants.Errors.EmptyEmail);
        }
        if (!content.UserEmail.IsValidEmail())
        {
            throw new EmailValidationException(content.UserEmail);
        }

        // Generate a unique instance ID for this approval workflow
        string instanceId = Guid.NewGuid().ToString();
        var options = new StartOrchestrationOptions
        {
            InstanceId = instanceId
        };

        // Start the orchestration with the user's email as input
        await client.ScheduleNewOrchestrationInstanceAsync(
            orchestratorName: TaskApprovalConstants.FunctionNames.TaskApprovalOrchestration,
            input: content.UserEmail,
            options: options);

        _logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Prepare the response content
        var responseContent = new
        {
            instanceId,
            userEmail = content.UserEmail,
            status = "Started"
        };

        // Create and return the HTTP response
        var response = _responseWriter.CreateResponse(req, HttpStatusCode.OK);
        await _responseWriter.WriteAsJsonAsync(response, responseContent);
        return response;
    }

    /// <summary>
    /// HTTP trigger function that approves a pending task.
    /// </summary>
    /// <param name="req">The HTTP request containing the instance ID to approve.</param>
    /// <param name="client">The durable task client used to raise the approval event.</param>
    /// <returns>HTTP response confirming the approval action.</returns>
    /// <exception cref="InvalidInputException">Thrown when the instance ID is empty or null.</exception>
    [Function("Approve")]
    public async Task<HttpResponseData> Approve(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        // Deserialize the request body
        var content = await _requestReader.ReadFromJsonAsync<ApprovalRequest>(req);
        if (content == null || string.IsNullOrEmpty(content.InstanceId))
        {
            throw new InvalidInputException(TaskApprovalConstants.Errors.EmptyInstanceId);
        }

        // Raise the approval event for the specified orchestration instance
        await client.RaiseEventAsync(content.InstanceId, TaskApprovalConstants.Events.ApprovalEvent,
            TaskApprovalConstants.Actions.Approve);

        // Create and return the HTTP response
        var response = _responseWriter.CreateResponse(req, HttpStatusCode.OK);
        await _responseWriter.WriteAsJsonAsync(response, new { message = TaskApprovalConstants.Success.Approved });
        return response;
    }

    /// <summary>
    /// HTTP trigger function that rejects a pending task.
    /// </summary>
    /// <param name="req">The HTTP request containing the instance ID to reject.</param>
    /// <param name="client">The durable task client used to raise the rejection event.</param>
    /// <returns>HTTP response confirming the rejection action.</returns>
    /// <exception cref="InvalidInputException">Thrown when the instance ID is empty or null.</exception>
    [Function("Reject")]
    public async Task<HttpResponseData> Reject(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        // Deserialize the request body
        var content = await _requestReader.ReadFromJsonAsync<ApprovalRequest>(req);
        if (content == null || string.IsNullOrEmpty(content.InstanceId))
        {
            throw new InvalidInputException(TaskApprovalConstants.Errors.EmptyInstanceId);
        }

        // Raise the rejection event for the specified orchestration instance
        await client.RaiseEventAsync(content.InstanceId, TaskApprovalConstants.Events.ApprovalEvent,
            TaskApprovalConstants.Actions.Reject);

        // Create and return the HTTP response
        var response = _responseWriter.CreateResponse(req, HttpStatusCode.OK);
        await _responseWriter.WriteAsJsonAsync(response, new { message = TaskApprovalConstants.Success.Rejected });
        return response;
    }

    /// <summary>
    /// Activity function that sends an email notification.
    /// </summary>
    /// <param name="request">The email request containing recipient, subject, and content.</param>
    /// <returns>True if the email was sent successfully; otherwise, false.</returns>
    [Function(TaskApprovalConstants.FunctionNames.SendEmail)]
    public async Task<bool> SendEmail([ActivityTrigger] EmailRequest request)
    {
        return await _emailService.SendEmailAsync(request);
    }
}