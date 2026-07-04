using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Application.DTOs.Incidents;

public record InjuryDto(
    Guid Id,
    string EmployeeName,
    string Department,
    string Location,
    string Section,
    DateTime InjuryOccurredAt,
    InjuryType InjuryType,
    BodyPartInjured BodyPartInjured,
    InjuryStatus Status,
    DateTime SubmittedAt,
    DateTime CreatedDate);

public record InjuryDetailDto(
    Guid Id,
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    Guid? EmployeeUserId,
    string EmployeeName,
    string Department,
    string Location,
    string Section,
    DateTime InjuryOccurredAt,
    InjuryType InjuryType,
    BodyPartInjured BodyPartInjured,
    string IncidentDescription,
    string? ImmediateActionTaken,
    string? RootCause,
    string? CorrectiveAction,
    int? LostTimeDays,
    string? Witnesses,
    InjuryStatus Status,
    DateTime SubmittedAt,
    Guid CapturedByUserId,
    IReadOnlyList<string> PhotoStoragePaths,
    IReadOnlyList<string> AttachmentStoragePaths,
    DateTime CreatedDate);

public record CreateInjuryDto(
    Guid? EmployeeUserId,
    string EmployeeName,
    string Department,
    string Location,
    string Section,
    DateTime InjuryOccurredAt,
    InjuryType InjuryType,
    BodyPartInjured BodyPartInjured,
    string IncidentDescription,
    string? ImmediateActionTaken,
    string? RootCause,
    string? CorrectiveAction,
    int? LostTimeDays,
    string? Witnesses,
    InjuryStatus Status,
    IReadOnlyList<string>? PhotoStoragePaths = null,
    IReadOnlyList<string>? AttachmentStoragePaths = null);

public record UpdateInjuryDto(
    string EmployeeName,
    string Department,
    string Location,
    string Section,
    InjuryType InjuryType,
    BodyPartInjured BodyPartInjured,
    string IncidentDescription,
    string? ImmediateActionTaken,
    string? RootCause,
    string? CorrectiveAction,
    int? LostTimeDays,
    string? Witnesses,
    InjuryStatus Status,
    IReadOnlyList<string>? PhotoStoragePaths = null,
    IReadOnlyList<string>? AttachmentStoragePaths = null);

public record InjuryFreeDaysDto(int InjuryFreeDays);

public record InjuryDashboardKpiDto(
    int InjuryFreeDays,
    int OpenNearMisses,
    int OpenCorrectiveActions,
    int AssessmentsSubmittedToday,
    int TotalEmployees,
    int NearMissesThisMonth,
    int LostTimeInjuries,
    int MedicalTreatmentInjuries,
    int FirstAidInjuries);

public record InjuryRegisterRowDto(
    Guid Id,
    DateTime InjuryOccurredAt,
    string EmployeeName,
    string Department,
    string Location,
    string Section,
    InjuryType InjuryType,
    BodyPartInjured BodyPartInjured,
    InjuryStatus Status);

public record InjuryTrendPointDto(string Label, int Count);

public record InjuryFrequencyRateDto(double Rate, int TotalInjuries, int TotalEmployees, string Period);

public record InjuryFreeDaysHistoryPointDto(string Label, int InjuryFreeDays);
