using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Application.DTOs.Safety;

public record StopUnsafeWorkDto(
    Guid Id,
    Guid ReportedByUserId,
    string Department,
    string Location,
    string Section,
    StopUnsafeWorkCategory Category,
    string Description,
    ImmediateRiskLevel ImmediateRisk,
    string? ActionsTaken,
    StopUnsafeWorkStatus Status,
    DateTime CreatedDate);

public record CreateStopUnsafeWorkDto(
    string Location,
    string Section,
    StopUnsafeWorkCategory Category,
    string Description,
    ImmediateRiskLevel ImmediateRisk,
    string? ActionsTaken,
    IReadOnlyList<string>? PhotoStoragePaths = null);

public record NearMissDetailDto(
    Guid Id,
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    string Department,
    string Location,
    string Section,
    NearMissCategory Category,
    string Description,
    DateTime OccurredAt,
    Guid ReportedByUserId,
    NearMissStatus Status,
    Guid? InvestigatorUserId,
    string? InvestigationNotes,
    string? RootCause,
    RootCauseCategory? RootCauseCategory,
    string? CorrectiveActionPlan,
    Guid? ResponsiblePersonUserId,
    DateTime? TargetDate,
    string? ClosureNotes,
    DateTime CreatedDate);

/// <param name="PhotoStoragePaths">Optional. Omit or pass null/empty when no photos are attached.</param>
public record CreateNearMissReportDto(
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    string Location,
    string Section,
    NearMissCategory Category,
    string Description,
    DateTime OccurredAt,
    IReadOnlyList<string>? PhotoStoragePaths = null);

public record InvestigateNearMissDto(
    Guid InvestigatorUserId,
    string InvestigationNotes,
    string RootCause,
    RootCauseCategory RootCauseCategory,
    string CorrectiveActionPlan,
    Guid ResponsiblePersonUserId,
    DateTime TargetDate);

public record CloseNearMissDto(string ClosureNotes);

public record SafetyNotificationDto(
    Guid Id,
    string Title,
    string Message,
    NotificationPriority Priority,
    WorkflowNotificationType NotificationType,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    bool IsRead,
    DateTime CreatedDate);

public record RegisterDeviceDto(string FcmToken, string Platform);

public record EmployeeSafetyKpiDto(
    int NearMissesSubmittedThisMonth,
    int MyDraftAssessments,
    int MySubmittedAssessments,
    int MyApprovedAssessments,
    int SafetyParticipationScore,
    int InjuryFreeDays);

public record ManagerSafetyKpiDto(
    int NearMissesThisMonth,
    int OpenNearMissInvestigations,
    int StopUnsafeWorkReports,
    int OpenCorrectiveActions,
    int PendingAssessments,
    int InjuryFreeDays,
    int OpenNearMisses,
    int AssessmentsSubmittedToday,
    int TotalEmployees,
    int LostTimeInjuries,
    int MedicalTreatmentInjuries,
    int FirstAidInjuries,
    IReadOnlyList<ChartPointDto> NearMissTrend,
    IReadOnlyList<NamedCountDto> NearMissesByDepartment,
    IReadOnlyList<NamedCountDto> NearMissesByCategory,
    IReadOnlyList<NamedCountDto> NearMissesBySeverity,
    IReadOnlyList<NamedCountDto> TopReportingDepartments);

public record ChartPointDto(string Label, int Value);

public record NamedCountDto(string Name, int Count);
