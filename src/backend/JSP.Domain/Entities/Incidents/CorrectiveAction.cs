using JSP.Domain.Common;
using JSP.Domain.Entities.Identity;

namespace JSP.Domain.Entities.Incidents;

public enum CorrectiveActionStatus
{
    Open = 0,
    InProgress = 1,
    Completed = 2,
    Verified = 3,
    Overdue = 4
}

public class CorrectiveAction : BaseEntity
{
    public Guid? IncidentId { get; set; }

    public Guid? NearMissId { get; set; }

    public string Description { get; set; } = string.Empty;

    public Guid AssignedToUserId { get; set; }

    public DateTime DueDate { get; set; }

    public CorrectiveActionStatus Status { get; set; } = CorrectiveActionStatus.Open;

    public DateTime? CompletedAt { get; set; }

    public Guid? VerifiedByUserId { get; set; }

    public Incident? Incident { get; set; }

    public NearMiss? NearMiss { get; set; }

    public User AssignedToUser { get; set; } = null!;

    public User? VerifiedByUser { get; set; }
}
