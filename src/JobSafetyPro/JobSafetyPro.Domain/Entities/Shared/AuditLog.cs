using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;

namespace JobSafetyPro.Domain.Entities.Shared;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }

    public Guid CompanyId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; }

    public User? User { get; set; }
}
