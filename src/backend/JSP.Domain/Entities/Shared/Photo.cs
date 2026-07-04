using JSP.Domain.Common;

namespace JSP.Domain.Entities.Shared;

public class Photo : BaseEntity
{
    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime CapturedAt { get; set; }
}
