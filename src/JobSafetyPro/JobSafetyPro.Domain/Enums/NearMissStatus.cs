namespace JobSafetyPro.Domain.Enums;

public enum NearMissStatus
{
    Draft = 0,
    Submitted = 1,
    UnderInvestigation = 2,
    CorrectiveActionAssigned = 3,
    AwaitingVerification = 4,
    Closed = 5,
}
