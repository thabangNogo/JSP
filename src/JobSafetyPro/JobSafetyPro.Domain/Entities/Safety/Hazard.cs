using JobSafetyPro.Domain.Common;

namespace JobSafetyPro.Domain.Entities.Safety;

public class Hazard : BaseEntity
{
    public Guid JobSafetyAssessmentId { get; set; }

    public Guid RiskLevelId { get; set; }

    public Guid? ResidualRiskLevelId { get; set; }

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public JobSafetyAssessment JobSafetyAssessment { get; set; } = null!;

    public RiskLevel RiskLevel { get; set; } = null!;

    public RiskLevel? ResidualRiskLevel { get; set; }

    public ICollection<ControlMeasure> ControlMeasures { get; set; } = new List<ControlMeasure>();
}
