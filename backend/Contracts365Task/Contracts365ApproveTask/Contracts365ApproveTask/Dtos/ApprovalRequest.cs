namespace Contracts365ApproveTask.Dtos;

public class ApprovalRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
}