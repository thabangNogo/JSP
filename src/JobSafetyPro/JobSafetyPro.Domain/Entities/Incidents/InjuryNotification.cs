using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Incidents;

public class InjuryNotification : BaseEntity
{
    public Guid InjuryId { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationPriority Priority { get; set; } = NotificationPriority.High;

    public DateTime SentAt { get; set; }

    public Injury Injury { get; set; } = null!;

    public User User { get; set; } = null!;
}
