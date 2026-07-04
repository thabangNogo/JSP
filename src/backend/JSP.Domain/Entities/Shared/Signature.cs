using JSP.Domain.Common;
using JSP.Domain.Entities.Identity;

namespace JSP.Domain.Entities.Shared;

public class Signature : BaseEntity
{
    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public Guid SignedByUserId { get; set; }

    public string SignatureDataPath { get; set; } = string.Empty;

    public DateTime SignedAt { get; set; }

    public string? IpAddress { get; set; }

    public User SignedByUser { get; set; } = null!;
}
