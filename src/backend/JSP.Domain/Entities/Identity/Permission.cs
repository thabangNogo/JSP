using JSP.Domain.Common;

namespace JSP.Domain.Entities.Identity;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
