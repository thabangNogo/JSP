using JobSafetyPro.Domain.Common;

namespace JobSafetyPro.Domain.Entities.MasterData;

public class WorkLocation : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
