using Contracts365ApproveTask.Dtos;

namespace Contracts365ApproveTask.Services
{
    /// <summary>
    /// Defines the contract for email notification services within the application.
    /// This interface standardizes how the application sends emails as part of the approval workflow,
    /// allowing for different email delivery implementations.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email based on the provided request details.
        /// </summary>
        /// <param name="request">The email request containing recipient, subject, and content.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a boolean indicating whether the email was sent successfully.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the request is null.</exception>
        /// <remarks>
        /// Implementations of this method should handle the actual email delivery logic,
        /// typically integrating with an email service provider like SendGrid, SMTP, etc.
        /// </remarks>
        Task<bool> SendEmailAsync(EmailRequest request);
    }
}
