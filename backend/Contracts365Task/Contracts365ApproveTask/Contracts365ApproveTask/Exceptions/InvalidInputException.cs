using System.Net;

namespace Contracts365ApproveTask.Exceptions;

/// <summary>
/// Exception thrown when the input provided to an operation is invalid (e.g., null or empty values).
/// </summary>
public class InvalidInputException : Contracts365ApproveTaskException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidInputException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InvalidInputException(string message)
        : base(message, HttpStatusCode.BadRequest)
    {
    }
}