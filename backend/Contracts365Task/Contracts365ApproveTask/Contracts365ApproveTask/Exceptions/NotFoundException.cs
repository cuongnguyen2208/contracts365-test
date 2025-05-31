using System.Net;

namespace Contracts365ApproveTask.Exceptions;

/// <summary>
/// Exception thrown when a requested resource or entity is not found.
/// </summary>
public class NotFoundException : Contracts365ApproveTaskException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public NotFoundException(string message)
        : base(message, HttpStatusCode.NotFound)
    {
    }
}