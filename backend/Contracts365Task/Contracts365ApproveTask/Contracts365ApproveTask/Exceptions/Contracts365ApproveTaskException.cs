using System.Net;

namespace Contracts365ApproveTask.Exceptions;

/// <summary>
/// Base exception class for all custom exceptions in the Contracts365 task approval application.
/// Provides an HTTP status code to standardize error responses.
/// </summary>
public abstract class Contracts365ApproveTaskException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with the exception.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Contracts365ApproveTaskException"/> class with a specified error message
    /// and HTTP status code.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="statusCode">The HTTP status code associated with the error (e.g., 400 for BadRequest).</param>
    protected Contracts365ApproveTaskException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Contracts365ApproveTaskException"/> class with a specified error message,
    /// an inner exception, and an HTTP status code.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    /// <param name="statusCode">The HTTP status code associated with the error.</param>
    protected Contracts365ApproveTaskException(string message, Exception innerException, HttpStatusCode statusCode)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}