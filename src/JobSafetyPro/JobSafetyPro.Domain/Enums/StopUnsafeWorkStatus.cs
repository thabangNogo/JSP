namespace JobSafetyPro.Domain.Enums;

public enum StopUnsafeWorkStatus
{
    Submitted = 0,
    Acknowledged = 1,
    WorkStopped = 2,
    Resolved = 3,
    Closed = 4,
}
