using JSP.Domain.Common;

namespace JSP.Domain.Entities.Identity;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? RelatedEntityType { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public User User { get; set; } = null!;
}
