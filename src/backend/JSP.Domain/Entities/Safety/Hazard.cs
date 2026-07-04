using JSP.Domain.Common;

namespace JSP.Domain.Entities.Safety;

public class Hazard : BaseEntity
{
    public Guid HazardCategoryId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public HazardCategory HazardCategory { get; set; } = null!;

    public ICollection<AssessmentHazard> AssessmentHazards { get; set; } = new List<AssessmentHazard>();
}
