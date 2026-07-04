using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Incidents;

public class CorrectiveAction : BaseEntity
{
    public Guid? IncidentId { get; set; }

    public Guid? NearMissId { get; set; }

    public string Description { get; set; } = string.Empty;

    public Guid AssignedToUserId { get; set; }

    public DateTime DueDate { get; set; }

    public CorrectiveActionStatus Status { get; set; } = CorrectiveActionStatus.Assigned;

    public DateTime? AssignedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid? VerifiedByUserId { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public Incident? Incident { get; set; }

    public NearMiss? NearMiss { get; set; }

    public User AssignedToUser { get; set; } = null!;

    public User? VerifiedByUser { get; set; }
}
