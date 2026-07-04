using JobSafetyPro.Domain.Common;

namespace JobSafetyPro.Domain.Entities.Identity;

public class UserDevice : BaseEntity
{
    public Guid UserId { get; set; }

    public string FcmToken { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public DateTime LastSeenAt { get; set; }

    public User User { get; set; } = null!;
}
