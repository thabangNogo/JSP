using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Organization;

namespace JobSafetyPro.Domain.Entities.Identity;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public Guid? PlantId { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;

    public Plant? Plant { get; set; }
}
