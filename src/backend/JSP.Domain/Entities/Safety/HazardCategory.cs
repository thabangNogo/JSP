using JSP.Domain.Common;

namespace JSP.Domain.Entities.Safety;

public class HazardCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public ICollection<Hazard> Hazards { get; set; } = new List<Hazard>();
}
