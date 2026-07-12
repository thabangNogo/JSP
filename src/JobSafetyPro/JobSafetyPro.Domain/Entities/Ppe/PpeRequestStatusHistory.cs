using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Ppe;

public class PpeRequestStatusHistory : BaseEntity
{
    public Guid PpeRequestId { get; set; }

    public PpeRequestStatus? OldStatus { get; set; }

    public PpeRequestStatus NewStatus { get; set; }

    public string Action { get; set; } = string.Empty;

    public Guid ActionByUserId { get; set; }

    public string ActionByUserName { get; set; } = string.Empty;

    public DateTime ActionDate { get; set; }

    public string? Comments { get; set; }

    public PpeRequest Request { get; set; } = null!;

    public User ActionByUser { get; set; } = null!;
}
