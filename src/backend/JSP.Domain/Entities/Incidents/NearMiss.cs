using JSP.Domain.Common;
using JSP.Domain.Entities.Identity;
using JSP.Domain.Entities.Organization;
using JSP.Domain.Entities.Safety;

namespace JSP.Domain.Entities.Incidents;

public class NearMiss : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid PlantId { get; set; }

    public Guid DepartmentId { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    public Guid ReportedByUserId { get; set; }

    public Guid? PotentialRiskLevelId { get; set; }

    public Company Company { get; set; } = null!;

    public Plant Plant { get; set; } = null!;

    public Department Department { get; set; } = null!;

    public User ReportedByUser { get; set; } = null!;

    public RiskLevel? PotentialRiskLevel { get; set; }

    public ICollection<CorrectiveAction> CorrectiveActions { get; set; } = new List<CorrectiveAction>();
}
