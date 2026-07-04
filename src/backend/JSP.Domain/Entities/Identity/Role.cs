using JSP.Domain.Common;
using JSP.Domain.Entities.Organization;

namespace JSP.Domain.Entities.Identity;

public class Role : BaseEntity
{
    public Guid? CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; }

    public Company? Company { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
