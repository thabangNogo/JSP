using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Incidents;

public class NearMiss : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid PlantId { get; set; }

    public Guid DepartmentId { get; set; }

    public string Department { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public NearMissCategory Category { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    public Guid ReportedByUserId { get; set; }

    public Guid? PotentialRiskLevelId { get; set; }

    public NearMissStatus Status { get; set; } = NearMissStatus.Submitted;

    public Guid? InvestigatorUserId { get; set; }

    public string? InvestigationNotes { get; set; }

    public string? RootCause { get; set; }

    public RootCauseCategory? RootCauseCategory { get; set; }

    public string? CorrectiveActionPlan { get; set; }

    public Guid? ResponsiblePersonUserId { get; set; }

    public DateTime? TargetDate { get; set; }

    public string? ClosureNotes { get; set; }

    public Company Company { get; set; } = null!;

    public Plant Plant { get; set; } = null!;

    public Department OrganizationDepartment { get; set; } = null!;

    public User ReportedByUser { get; set; } = null!;

    public User? InvestigatorUser { get; set; }

    public User? ResponsiblePersonUser { get; set; }

    public RiskLevel? PotentialRiskLevel { get; set; }

    public ICollection<CorrectiveAction> CorrectiveActions { get; set; } = new List<CorrectiveAction>();
}
