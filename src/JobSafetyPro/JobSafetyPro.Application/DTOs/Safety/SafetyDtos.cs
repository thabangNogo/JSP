using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Application.DTOs.Safety;

public record JobSafetyAssessmentDto(
    Guid Id,
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    string Title,
    string JobDescription,
    string Department,
    string Location,
    string Section,
    Guid? WorkLocationId,
    Guid? WorkSectionId,
    JsaStatus Status,
    int CurrentStep,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime? LastSavedAt,
    string? SignOffName,
    string? SignOffSurname,
    string? SignOffCompanyNumber,
    string? SignOffOccupation,
    string? SignatureStoragePath,
    IReadOnlyList<HazardDto> Hazards,
    IReadOnlyList<ControlMeasureDto> ControlMeasures);

public record JobSafetyAssessmentSummaryDto(
    Guid Id,
    string Title,
    JsaStatus Status,
    int CurrentStep,
    DateTime? LastSavedAt,
    DateTime CreatedDate,
    string Department,
    string Location,
    string Section,
    Guid? WorkLocationId,
    Guid? WorkSectionId);

public record CreateJobSafetyAssessmentDto(
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    string Title,
    string JobDescription,
    Guid WorkLocationId,
    Guid WorkSectionId,
    string? ObserveStopNotes = null,
    string? ConversationNotes = null,
    string? SignatureStoragePath = null,
    string? SignOffName = null,
    string? SignOffSurname = null,
    string? SignOffCompanyNumber = null,
    string? SignOffOccupation = null,
    IReadOnlyList<CreateJobSafetyAssessmentHazardDto>? Hazards = null,
    IReadOnlyList<CreateJobSafetyAssessmentControlDto>? Controls = null);

public record SaveAssessmentDraftDto(
    Guid? Id,
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    string Title,
    string JobDescription,
    Guid WorkLocationId,
    Guid WorkSectionId,
    int CurrentStep,
    string? WorkflowDataJson = null,
    string? SignatureStoragePath = null,
    string? SignOffName = null,
    string? SignOffSurname = null,
    string? SignOffCompanyNumber = null,
    string? SignOffOccupation = null,
    IReadOnlyList<CreateJobSafetyAssessmentHazardDto>? Hazards = null,
    IReadOnlyList<CreateJobSafetyAssessmentControlDto>? Controls = null);

public record CreateJobSafetyAssessmentHazardDto(
    string Description,
    Guid? RiskLevelId,
    Guid? ResidualRiskLevelId,
    int SortOrder,
    string? ClientHazardId = null);

public record CreateJobSafetyAssessmentControlDto(
    string Description,
    string HierarchyOfControl,
    bool IsImplemented = false,
    string? ClientHazardId = null);

public record UpdateJobSafetyAssessmentDto(
    string Title,
    string JobDescription,
    JsaStatus Status);

public record HazardDto(
    Guid Id,
    Guid JobSafetyAssessmentId,
    Guid RiskLevelId,
    Guid? ResidualRiskLevelId,
    string Description,
    int SortOrder);

public record CreateHazardDto(
    Guid JobSafetyAssessmentId,
    Guid RiskLevelId,
    Guid? ResidualRiskLevelId,
    string Description,
    int SortOrder);

public record RiskLevelDto(Guid Id, string Code, string Name, int NumericValue, string ColorHex);

public record ControlMeasureDto(
    Guid Id,
    Guid JobSafetyAssessmentId,
    Guid? HazardId,
    string Description,
    string HierarchyOfControl,
    bool IsImplemented);

public record CreateControlMeasureDto(
    Guid JobSafetyAssessmentId,
    Guid? HazardId,
    string Description,
    string HierarchyOfControl,
    bool IsImplemented);
