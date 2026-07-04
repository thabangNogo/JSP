using JSP.Domain.Common;

namespace JSP.Domain.Entities.Safety;

public class ControlMeasure : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<AssessmentControl> AssessmentControls { get; set; } = new List<AssessmentControl>();
}
