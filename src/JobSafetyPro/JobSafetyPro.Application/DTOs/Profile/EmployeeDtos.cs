namespace JobSafetyPro.Application.DTOs.Profile;

public record EmployeeListItemDto(
    Guid Id,
    string EmployeeNumber,
    string Name,
    string Surname,
    string Department,
    string Occupation,
    int AssessmentsCompleted,
    int NearMissesSubmitted,
    string Status,
    bool IsActive);

public record EmployeeDetailDto(
    Guid Id,
    string Email,
    string EmployeeNumber,
    string Name,
    string Surname,
    string Department,
    string Occupation,
    string CompanyNumber,
    DateTime DateJoined,
    string Status,
    bool IsActive,
    IReadOnlyList<EmployeeAssessmentDto> Assessments,
    IReadOnlyList<EmployeeNearMissDto> NearMisses,
    IReadOnlyList<EmployeeCorrectiveActionDto> CorrectiveActions,
    IReadOnlyList<EmployeeActivityDto> ActivityTimeline);

public record EmployeeAssessmentDto(
    Guid Id,
    string Title,
    string Status,
    DateTime CreatedDate,
    DateTime? CompletedDate);

public record EmployeeNearMissDto(
    Guid Id,
    DateTime Date,
    string Category,
    string Location,
    string Status);

public record EmployeeCorrectiveActionDto(
    Guid Id,
    string Description,
    DateTime AssignedDate,
    DateTime DueDate,
    string Status);

public record EmployeeActivityDto(
    DateTime OccurredAt,
    string ActivityType,
    string Description);

public record EmployeeStatsDto(
    int TotalEmployees,
    int EmployeesOnline,
    int AssessmentsCompletedToday,
    int NearMissesThisMonth,
    double AverageAssessmentsPerEmployee);

public record CreateEmployeeDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    Guid WorkDepartmentId,
    string CompanyNumber,
    string Occupation,
    IReadOnlyList<string> Roles);

public record EmployeeSearchQuery(
    string? Search = null,
    string? Department = null,
    string? Occupation = null,
    int Page = 1,
    int PageSize = 20);
