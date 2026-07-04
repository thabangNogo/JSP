namespace JobSafetyPro.Application.DTOs.Profile;

public record EmployeeProfileDto(
    Guid Id,
    Guid UserId,
    Guid WorkDepartmentId,
    string WorkDepartmentName,
    string Name,
    string Surname,
    string CompanyNumber,
    string Occupation);

public record SaveEmployeeProfileDto(
    Guid WorkDepartmentId,
    string Name,
    string Surname,
    string CompanyNumber,
    string Occupation);
