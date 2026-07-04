using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Application.DTOs.Workflow;

public record RejectJsaDto(string RejectionReason);

public record JsaReviewDto(
    Guid Id,
    string Title,
    string JobDescription,
    string Department,
    string Location,
    string Section,
    JsaStatus Status,
    string? EmployeeName,
    string? SignOffName,
    string? SignOffSurname,
    string? SignOffCompanyNumber,
    string? SignOffOccupation,
    string? SignatureStoragePath,
    string? RejectionReason,
    IReadOnlyList<HazardDto> Hazards,
    IReadOnlyList<ControlMeasureDto> ControlMeasures);

public record AcknowledgeStopUnsafeWorkDto(string? Notes);

public record ResolveStopUnsafeWorkDto(string CorrectiveActionNotes, string? ActionsTaken);

public record VerifyStopUnsafeWorkDto(string? Notes);

public record InjuryInvestigationDto(string RootCause, string CorrectiveAction);

public record InjuryMedicalOutcomeDto(string MedicalOutcome);

public record InjuryReturnToWorkDto(DateTime ReturnToWorkDate);

public record CloseInjuryDto(string? ClosureNotes);

public record CompleteCorrectiveActionDto(string? Notes);

public record VerifyCorrectiveActionDto(string? Notes);

public record ManagerPendingActionsDto(
    int PendingJsaApprovals,
    int NearMissesAwaitingInvestigation,
    int CorrectiveActionsOverdue,
    int OpenInjuries,
    int OpenUnsafeWorkReports,
    int PendingJsas,
    IReadOnlyList<PendingActionItemDto> Items);

public record PendingActionItemDto(
    string Module,
    Guid Id,
    string Title,
    string Status,
    DateTime CreatedDate);

public record NotificationSummaryDto(int UnreadCount);

public record AuditLogDto(
    Guid Id,
    string Action,
    string EntityType,
    Guid EntityId,
    Guid? UserId,
    string? OldValues,
    string? NewValues,
    DateTime Timestamp);
