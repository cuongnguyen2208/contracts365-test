namespace Contracts365ApproveTask.Exceptions;

/// <summary>
/// Exception thrown when an invalid approval event is received during task orchestration.
/// </summary>
public class InvalidApprovalEventException : InvalidInputException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidApprovalEventException"/> class with the invalid event value.
    /// </summary>
    /// <param name="eventValue">The invalid event value that caused the exception.</param>
    public InvalidApprovalEventException(string eventValue)
        : base($"Invalid approval event: {eventValue}")
    {
    }
}