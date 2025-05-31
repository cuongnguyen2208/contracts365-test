using System.Net;

namespace Contracts365ApproveTask.Exceptions;

/// <summary>
/// Exception thrown when an error occurs during the email sending process.
/// </summary>
public class EmailSendingException : Contracts365ApproveTaskException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSendingException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The inner exception that caused this exception, or null if none.</param>
    public EmailSendingException(string message, Exception innerException = null)
        : base(message, innerException, HttpStatusCode.InternalServerError)
    {
    }
}