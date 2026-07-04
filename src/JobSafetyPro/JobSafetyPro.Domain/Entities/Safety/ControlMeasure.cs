using JobSafetyPro.Domain.Common;

namespace JobSafetyPro.Domain.Entities.Safety;

public class ControlMeasure : BaseEntity
{
    public Guid JobSafetyAssessmentId { get; set; }

    public Guid? HazardId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string HierarchyOfControl { get; set; } = string.Empty;

    public bool IsImplemented { get; set; }

    public JobSafetyAssessment JobSafetyAssessment { get; set; } = null!;

    public Hazard? Hazard { get; set; }
}
