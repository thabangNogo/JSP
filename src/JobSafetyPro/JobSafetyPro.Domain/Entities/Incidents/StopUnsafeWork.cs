using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Incidents;

public class StopUnsafeWork : BaseEntity
{
    public Guid ReportedByUserId { get; set; }

    public string Department { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public StopUnsafeWorkCategory Category { get; set; }

    public string Description { get; set; } = string.Empty;

    public ImmediateRiskLevel ImmediateRisk { get; set; }

    public string? ActionsTaken { get; set; }

    public StopUnsafeWorkStatus Status { get; set; } = StopUnsafeWorkStatus.Submitted;

    public Guid? AcknowledgedByUserId { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    public DateTime? WorkStoppedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public Guid? ResolvedByUserId { get; set; }

    public string? CorrectiveActionNotes { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public Guid? VerifiedByUserId { get; set; }

    public DateTime? ClosedAt { get; set; }

    public User ReportedByUser { get; set; } = null!;
}
