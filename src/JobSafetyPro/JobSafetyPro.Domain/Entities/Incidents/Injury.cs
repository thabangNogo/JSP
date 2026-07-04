using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Incidents;

public class Injury : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid PlantId { get; set; }

    public Guid DepartmentId { get; set; }

    public Guid? EmployeeUserId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public DateTime InjuryOccurredAt { get; set; }

    /// <summary>
    /// Immutable anchor for injury-free-days calculation. Set once at submission.
    /// </summary>
    public DateTime InjuryFreeDaysResetDate { get; set; }

    public DateTime SubmittedAt { get; set; }

    public bool IsSubmitted { get; set; } = true;

    public InjuryType InjuryType { get; set; }

    public BodyPartInjured BodyPartInjured { get; set; }

    public string IncidentDescription { get; set; } = string.Empty;

    public string? ImmediateActionTaken { get; set; }

    public string? RootCause { get; set; }

    public string? CorrectiveAction { get; set; }

    public int? LostTimeDays { get; set; }

    public string? Witnesses { get; set; }

    public InjuryStatus Status { get; set; } = InjuryStatus.Open;

    public Guid? InvestigatedByUserId { get; set; }

    public DateTime? InvestigatedAt { get; set; }

    public string? MedicalOutcome { get; set; }

    public DateTime? ReturnToWorkDate { get; set; }

    public DateTime? ClosedAt { get; set; }

    public Guid? ClosedByUserId { get; set; }

    public Guid CapturedByUserId { get; set; }

    public Company Company { get; set; } = null!;

    public Plant Plant { get; set; } = null!;

    public Department OrganizationDepartment { get; set; } = null!;

    public User? EmployeeUser { get; set; }

    public User CapturedByUser { get; set; } = null!;

    public ICollection<InjuryPhoto> Photos { get; set; } = new List<InjuryPhoto>();

    public ICollection<InjuryNotification> Notifications { get; set; } = new List<InjuryNotification>();
}
