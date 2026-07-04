using JSP.Domain.Common;

namespace JSP.Domain.Entities.Safety;

public class AssessmentHazard : BaseEntity
{
    public Guid JobSafetyAssessmentId { get; set; }

    public Guid HazardId { get; set; }

    public Guid InitialRiskLevelId { get; set; }

    public Guid? ResidualRiskLevelId { get; set; }

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public JobSafetyAssessment JobSafetyAssessment { get; set; } = null!;

    public Hazard Hazard { get; set; } = null!;

    public RiskLevel InitialRiskLevel { get; set; } = null!;

    public RiskLevel? ResidualRiskLevel { get; set; }

    public ICollection<AssessmentControl> AssessmentControls { get; set; } = new List<AssessmentControl>();
}
