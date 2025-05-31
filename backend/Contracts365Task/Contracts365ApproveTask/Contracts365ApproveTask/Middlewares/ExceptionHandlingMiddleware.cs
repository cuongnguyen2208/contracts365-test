using Contracts365ApproveTask.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Contracts365ApproveTask.Middlewares;

/// <summary>
/// Middleware that provides centralized exception handling for Azure Functions.
/// This middleware intercepts exceptions thrown during function execution and converts them 
/// into appropriate HTTP responses with meaningful status codes and error messages.
/// </summary>
/// <remarks>
/// This middleware should be registered in the Function host builder configuration to handle
/// exceptions across all function executions. It specifically handles HTTP-triggered functions
/// by producing appropriate HTTP responses based on the exception type.
/// 
/// Exception handling strategy:
/// - Contracts365ApproveTaskException: Returns the status code defined in the exception
/// - ArgumentNullException: Returns 400 Bad Request with parameter information
/// - ArgumentException: Returns 400 Bad Request
/// - Other exceptions: Returns 500 Internal Server Error
/// 
/// Non-HTTP triggered functions will have their exceptions re-thrown.
/// </remarks>
public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording exception details and diagnostics.</param>
    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle exceptions in the Azure Functions execution pipeline.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <param name="next">Delegate to execute the next middleware in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method wraps the function execution in a try-catch block and handles any exceptions
    /// by converting them to appropriate HTTP responses for HTTP-triggered functions.
    /// </remarks>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception encountered: {Message}", ex.Message);

            var httpReqData = await context.GetHttpRequestDataAsync();
            if (httpReqData != null)
            {
                var response = HandleException(ex, httpReqData);
                context.GetInvocationResult().Value = response;
            }
            else
            {
                // Re-throw if it's not an HTTP trigger
                throw;
            }
        }
    }

    /// <summary>
    /// Creates an appropriate HTTP response based on the exception type.
    /// </summary>
    /// <param name="exception">The exception that was thrown during function execution.</param>
    /// <param name="httpRequest">The original HTTP request data.</param>
    /// <returns>An HTTP response with appropriate status code and error message.</returns>
    /// <remarks>
    /// This method maps different exception types to specific HTTP status codes and
    /// generates error messages that are safe to return to clients.
    /// </remarks>
    private HttpResponseData HandleException(Exception exception, HttpRequestData httpRequest)
    {
        HttpResponseData response;

        switch (exception)
        {
            case Contracts365ApproveTaskException taskException:
                response = httpRequest.CreateResponse(taskException.StatusCode);
                response.WriteAsJsonAsync(new { message = taskException.Message }).GetAwaiter().GetResult();
                break;

            case ArgumentNullException argNullEx:
                response = httpRequest.CreateResponse(HttpStatusCode.BadRequest);
                response.WriteAsJsonAsync(new { message = $"Required parameter missing: {argNullEx.ParamName}" }).GetAwaiter().GetResult();
                break;

            case ArgumentException argEx:
                response = httpRequest.CreateResponse(HttpStatusCode.BadRequest);
                response.WriteAsJsonAsync(new { message = argEx.Message }).GetAwaiter().GetResult();
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                response = httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
                response.WriteAsJsonAsync(new { message = "An unexpected error occurred." }).GetAwaiter().GetResult();
                break;
        }

        return response;
    }
}