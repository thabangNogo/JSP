using JSP.Domain.Common;

namespace JSP.Domain.Entities.Safety;

public class AssessmentControl : BaseEntity
{
    public Guid AssessmentHazardId { get; set; }

    public Guid ControlMeasureId { get; set; }

    public Guid HierarchyOfControlId { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsImplemented { get; set; }

    public AssessmentHazard AssessmentHazard { get; set; } = null!;

    public ControlMeasure ControlMeasure { get; set; } = null!;

    public HierarchyOfControl HierarchyOfControl { get; set; } = null!;
}
