using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Identity;

public class SafetyNotification : BaseEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public WorkflowNotificationType NotificationType { get; set; } = WorkflowNotificationType.General;

    public string? RelatedEntityType { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
}
