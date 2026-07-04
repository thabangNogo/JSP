using JSP.Domain.Common;

namespace JSP.Domain.Entities.Safety;

public class HierarchyOfControl : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int PriorityOrder { get; set; }

    public string Description { get; set; } = string.Empty;

    public ICollection<AssessmentControl> AssessmentControls { get; set; } = new List<AssessmentControl>();
}
