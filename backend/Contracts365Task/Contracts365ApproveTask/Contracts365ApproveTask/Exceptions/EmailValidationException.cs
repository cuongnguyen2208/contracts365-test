namespace Contracts365ApproveTask.Exceptions;

/// <summary>
/// Exception thrown when an email address fails validation (e.g., invalid format).
/// </summary>
public class EmailValidationException : InvalidInputException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailValidationException"/> class with the invalid email address.
    /// </summary>
    /// <param name="email">The email address that failed validation.</param>
    public EmailValidationException(string email)
        : base($"Invalid email format: {email}")
    {
    }
}