namespace Contracts365ApproveTask.Constants;

public static class TaskApprovalConstants
{
    public static class Errors
    {
        public const string EmptyEmail = "User email cannot be null or empty.";
        public const string EmptyInstanceId = "Instance ID is required.";
    }

    public static class Success
    {
        public const string Approved = "Approval event sent.";
        public const string Rejected = "Rejection event sent.";
    }

    public static class Events
    {
        public const string ApprovalEvent = "ApprovalEvent";
    }

    public static class Actions
    {
        public const string Approve = "Approve";
        public const string Reject = "Reject";
    }

    public static class FunctionNames
    {
        public const string SendEmail = "SendEmail";
        public const string TaskApprovalOrchestration = "TaskApprovalOrchestration";
    }

    public static class EmailTemplates
    {
        public static class Subjects
        {
            public const string TaskApprovalStarted = "Task Approval Started";
            public const string TaskApproved = "Task Approved";
            public const string TaskRejected = "Task Rejected";
        }

        public static class Contents
        {
            public const string TaskApprovalStarted = "Your task approval process has started.";
            public const string TaskApproved = "Your task has been approved.";
            public const string TaskRejected = "Your task has been rejected.";
        }
    }
}