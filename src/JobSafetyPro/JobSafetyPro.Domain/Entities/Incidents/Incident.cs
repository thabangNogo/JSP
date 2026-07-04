using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Incidents;

public class Incident : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid PlantId { get; set; }

    public Guid DepartmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IncidentSeverity Severity { get; set; }

    public IncidentStatus Status { get; set; } = IncidentStatus.Reported;

    public DateTime OccurredAt { get; set; }

    public Guid ReportedByUserId { get; set; }

    public Guid? InvestigatedByUserId { get; set; }

    public Company Company { get; set; } = null!;

    public Plant Plant { get; set; } = null!;

    public Department Department { get; set; } = null!;

    public User ReportedByUser { get; set; } = null!;

    public User? InvestigatedByUser { get; set; }

    public ICollection<CorrectiveAction> CorrectiveActions { get; set; } = new List<CorrectiveAction>();
}
