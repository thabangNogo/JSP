using JSP.Domain.Common;
using JSP.Domain.Entities.Organization;

namespace JSP.Domain.Entities.Identity;

public class User : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid? PlantId { get; set; }

    public Guid? DepartmentId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;

    public Plant? Plant { get; set; }

    public Department? Department { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
