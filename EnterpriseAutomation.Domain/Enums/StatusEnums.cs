namespace EnterpriseAutomation.Domain.Entities.Enums
{
    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public enum RequestStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Rejected = 4,
        Cancelled = 5
    }
}
