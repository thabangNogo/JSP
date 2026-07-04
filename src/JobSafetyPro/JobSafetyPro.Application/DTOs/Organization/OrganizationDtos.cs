namespace JobSafetyPro.Application.DTOs.Organization;

public record CompanyDto(Guid Id, string Name, string Code, bool IsActive);

public record CreateCompanyDto(string Name, string Code);

public record PlantDto(Guid Id, Guid CompanyId, string Name, string Code, string TimeZone, bool IsActive);

public record CreatePlantDto(Guid CompanyId, string Name, string Code, string TimeZone);

public record DepartmentDto(Guid Id, Guid PlantId, string Name, string Code, bool IsActive);

public record CreateDepartmentDto(Guid PlantId, string Name, string Code);

public record UserDto(
    Guid Id,
    Guid CompanyId,
    Guid? PlantId,
    Guid? DepartmentId,
    string Email,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    bool IsActive,
    IReadOnlyList<string> Roles);

public record CreateUserDto(
    Guid CompanyId,
    Guid? PlantId,
    Guid? DepartmentId,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    IReadOnlyList<string> Roles);

public record RoleDto(Guid Id, string Name, string Description, bool IsSystemRole);
