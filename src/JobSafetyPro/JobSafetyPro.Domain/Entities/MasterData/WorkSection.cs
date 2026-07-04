using JobSafetyPro.Domain.Common;

namespace JobSafetyPro.Domain.Entities.MasterData;

public class WorkSection : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
