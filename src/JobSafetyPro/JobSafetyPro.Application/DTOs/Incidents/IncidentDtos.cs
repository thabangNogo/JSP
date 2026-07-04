using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Application.DTOs.Incidents;

public record IncidentDto(
    Guid Id,
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    string Title,
    string Description,
    IncidentSeverity Severity,
    IncidentStatus Status,
    DateTime OccurredAt,
    Guid ReportedByUserId);

public record CreateIncidentDto(
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    string Title,
    string Description,
    IncidentSeverity Severity,
    DateTime OccurredAt);

public record NearMissDto(
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
    NearMissStatus Status,
    Guid? PotentialRiskLevelId);

public record CreateNearMissDto(
    Guid CompanyId,
    Guid PlantId,
    Guid DepartmentId,
    string Description,
    DateTime OccurredAt,
    Guid? PotentialRiskLevelId);

public record CorrectiveActionDto(
    Guid Id,
    Guid? IncidentId,
    Guid? NearMissId,
    string Description,
    Guid AssignedToUserId,
    DateTime DueDate,
    CorrectiveActionStatus Status);

public record CreateCorrectiveActionDto(
    Guid? IncidentId,
    Guid? NearMissId,
    string Description,
    Guid AssignedToUserId,
    DateTime DueDate);

public record AttachmentDto(
    Guid Id,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string FileName,
    string ContentType,
    string StoragePath,
    long FileSizeBytes);

public record CreateAttachmentDto(
    string RelatedEntityType,
    Guid RelatedEntityId,
    string FileName,
    string ContentType,
    string StoragePath,
    long FileSizeBytes);
