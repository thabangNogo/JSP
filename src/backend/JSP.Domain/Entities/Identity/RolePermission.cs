using JSP.Domain.Common;

namespace JSP.Domain.Entities.Identity;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
