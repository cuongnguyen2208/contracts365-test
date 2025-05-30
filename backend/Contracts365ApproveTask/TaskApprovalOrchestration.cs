using Contracts365ApproveTask.Dtos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Contracts365ApproveTask;

public static class TaskApprovalOrchestration
{
    [FunctionName("TaskApprovalOrchestration")]
    public static async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var userEmail = context.GetInput<string>();
        if (string.IsNullOrEmpty(userEmail))
        {
            throw new ArgumentException("User email cannot be null or empty.");
        }

        if (!IsValidEmail(userEmail))
        {
            throw new ArgumentException("Invalid email format.");
        }

        await context.CallActivityAsync("SendEmail", new EmailRequest
        {
            Email = userEmail,
            Subject = "Start Approve Task",
            Content = "Your task is started to approve."
        });

        var approvalEvent = await context.WaitForExternalEvent<string>("ApprovalEvent");
        if (approvalEvent == "Approve")
        {
            await context.CallActivityAsync("SendEmail", new EmailRequest
            {
                Email = userEmail,
                Subject = "Approve completed",
                Content = "Your task is approved."
            });
            return "Approved";
        }
        else if (approvalEvent == "Reject")
        {
            await context.CallActivityAsync("SendEmail", new EmailRequest
            {
                Email = userEmail,
                Subject = "Approve rejected",
                Content = "Your task is rejected."
            });
            return "Rejected";
        }
        else
        {
            throw new ArgumentException($"Invalid approval event: {approvalEvent}");
        }
    }

    /// <summary>
    /// Sends an start approval event to the orchestration.
    /// </summary>
    [FunctionName("StartApproval")]
    public static async Task<HttpResponseMessage> StartApproval(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
                [DurableClient] IDurableOrchestrationClient client)
    {
        try
        {
            var content = await req.Content.ReadAsAsync<ApprovalRequest>();
            if (string.IsNullOrEmpty(content.UserEmail))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { message = "User email is required." }),
                        Encoding.UTF8,
                        "application/json")
                };
            }

            string instanceId = Guid.NewGuid().ToString();
            await client.StartNewAsync("TaskApprovalOrchestration", instanceId, content.UserEmail);
            var responseContent = new
            {
                instanceId,
                userEmail = content.UserEmail,
                status = "Started"
            };
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(responseContent),
                    Encoding.UTF8,
                    "application/json")
            };
        }
        catch (Exception ex)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { message = $"Error starting approval: {ex.Message}" }),
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }

    /// <summary>
    /// Sends an approval event to the orchestration.
    /// </summary>
    [FunctionName("Approve")]
    public static async Task<HttpResponseMessage> Approve(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient client)
    {
        try
        {
            var content = await req.Content.ReadAsAsync<ApprovalRequest>();
            if (string.IsNullOrEmpty(content.InstanceId))
            {
                var responseErrorContent = new { message = "Instance ID is required." };
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonSerializer.Serialize(responseErrorContent), Encoding.UTF8, "application/json")
                };
            }

            await client.RaiseEventAsync(content.InstanceId, "ApprovalEvent", "Approve");
            var responseContent = new { message = "Approval event sent." };
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
            };
        }
        catch (Exception ex)
        {
            var responseErrorContent = new { message = $"Error approving: {ex.Message}" };
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(JsonSerializer.Serialize(responseErrorContent), Encoding.UTF8, "application/json")
            };
        }
    }

    /// <summary>
    /// Sends a rejection event to the orchestration.
    /// </summary>
    [FunctionName("Reject")]
    public static async Task<HttpResponseMessage> Reject(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient client)
    {
        try
        {
            var content = await req.Content.ReadAsAsync<ApprovalRequest>();
            if (string.IsNullOrEmpty(content.InstanceId))
            {
                var responseErrorContent = new { message = "Instance ID is required." };
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonSerializer.Serialize(responseErrorContent), Encoding.UTF8, "application/json")
                };
            }

            await client.RaiseEventAsync(content.InstanceId, "ApprovalEvent", "Reject");
            var responseContent = new { message = "Rejection event sent." };
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
            };
        }
        catch (Exception ex)
        {
            var responseErrorContent = new { message = $"Error rejecting: {ex.Message}" };
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(JsonSerializer.Serialize(responseErrorContent), Encoding.UTF8, "application/json")
            };
        }
    }

    [FunctionName("SendEmail")]
    public static Task<bool> SendEmail([ActivityTrigger] EmailRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        if (string.IsNullOrEmpty(request.Email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(request));
        }
        if (!IsValidEmail(request.Email))
        {
            throw new ArgumentException("Invalid email format.", nameof(request.Email));
        }

        Console.WriteLine($"Simulated email sent to {request.Email} with subject: {request.Subject}, content: {request.Content}");
        return Task.FromResult(true);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}