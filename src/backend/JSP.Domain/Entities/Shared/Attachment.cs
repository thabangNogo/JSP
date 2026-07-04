using JSP.Domain.Common;

namespace JSP.Domain.Entities.Shared;

public class Attachment : BaseEntity
{
    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
}
