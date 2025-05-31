namespace Contracts365ApproveTask.Helpers;

/// <summary>
/// Provides utility methods for email-related operations.
/// This static class contains extension methods that help validate and process email addresses
/// throughout the application.
/// </summary>
/// <remarks>
/// EmailHelper encapsulates common email validation logic to ensure consistency
/// across the application when working with email addresses.
/// </remarks>
public static class EmailHelper
{
    /// <summary>
    /// Validates whether a string is a properly formatted email address.
    /// </summary>
    /// <param name="email">The string to validate as an email address.</param>
    /// <returns>
    /// <c>true</c> if the string is a valid email address; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method uses the <see cref="System.Net.Mail.MailAddress"/> class to validate 
    /// the email format. It attempts to create a mail address object with the provided string,
    /// which performs syntax validation according to RFC standards.
    /// 
    /// The method also ensures that the parsed address exactly matches the input string
    /// to avoid partial matches. If any exception occurs during validation, the method
    /// returns <c>false</c>.
    /// </remarks>
    public static bool IsValidEmail(this string email)
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