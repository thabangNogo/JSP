using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Organization;

namespace JobSafetyPro.Domain.Entities.Identity;

public class Role : BaseEntity
{
    public Guid? CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; }

    public Company? Company { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
