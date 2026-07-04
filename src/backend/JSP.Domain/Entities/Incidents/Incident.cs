using JSP.Domain.Common;
using JSP.Domain.Entities.Identity;
using JSP.Domain.Entities.Organization;
namespace JSP.Domain.Entities.Incidents;

public enum IncidentSeverity
{
    Minor = 0,
    Moderate = 1,
    Major = 2,
    Critical = 3
}

public enum IncidentStatus
{
    Reported = 0,
    UnderInvestigation = 1,
    CorrectiveAction = 2,
    Closed = 3
}

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
