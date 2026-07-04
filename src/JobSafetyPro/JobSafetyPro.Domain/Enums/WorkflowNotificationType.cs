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
    General = 99,
}
