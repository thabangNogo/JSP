using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.MasterData;

namespace JobSafetyPro.Domain.Entities.Identity;

public class EmployeeProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid WorkDepartmentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string CompanyNumber { get; set; } = string.Empty;

    public string Occupation { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    public WorkDepartment WorkDepartment { get; set; } = null!;
}
