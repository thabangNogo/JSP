using JobSafetyPro.Domain.Common;

namespace JobSafetyPro.Domain.Entities.Incidents;

public class InjuryPhoto : BaseEntity
{
    public Guid InjuryId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "image/jpeg";

    public string StoragePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public Injury Injury { get; set; } = null!;
}
