namespace JobSafetyPro.Domain.Enums;

public enum WorkflowNotificationType
{
    AssessmentSubmitted = 1,
    AssessmentApproved = 2,
    AssessmentRejected = 3,
    NearMissSubmitted = 4,
    NearMissAssigned = 5,
    CorrectiveActionAssigned = 6,
    CorrectiveActionCompleted = 7,
    CorrectiveActionOverdue = 8,
    UnsafeWorkReported = 9,
    InjuryCaptured = 10,
    PpeRequested = 11,
    PpeApproved = 12,
    PpeRejected = 13,
    PpePreparing = 14,
    PpeDispatched = 15,
    PpeReadyForCollection = 16,
    PpeCollected = 17,
    PpeStockLow = 18,
    PpeOverdue = 19,
    PpeWaitingEscalation = 20,
    General = 99,
}
