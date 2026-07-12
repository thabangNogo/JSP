using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.MasterData;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Ppe;

public class PpeRequest : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;

    public Guid CompanyId { get; set; }

    public Guid EmployeeUserId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public Guid? WorkDepartmentId { get; set; }

    public string Department { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public Guid PpeCatalogueItemId { get; set; }

    public int Quantity { get; set; }

    public string Reason { get; set; } = string.Empty;

    public PpeRequestPriority Priority { get; set; } = PpeRequestPriority.Medium;

    public DateTime RequestedDate { get; set; }

    public DateTime RequiredByDate { get; set; }

    public string? Comments { get; set; }

    public PpeRequestStatus Status { get; set; } = PpeRequestStatus.Requested;

    public Guid RequestedByUserId { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? DispatchDate { get; set; }

    public Guid? IssuedByUserId { get; set; }

    public string? CollectedByEmployee { get; set; }

    public string? EmployeeSignature { get; set; }

    public string? SafetyOfficerSignature { get; set; }

    public string? DispatchNotes { get; set; }

    public DateTime? CollectedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? ArchivedDate { get; set; }

    public User EmployeeUser { get; set; } = null!;

    public User RequestedByUser { get; set; } = null!;

    public User? IssuedByUser { get; set; }

    public User? ApprovedByUser { get; set; }

    public WorkDepartment? WorkDepartment { get; set; }

    public PpeCatalogueItem CatalogueItem { get; set; } = null!;

    public ICollection<PpeRequestStatusHistory> StatusHistory { get; set; } = new List<PpeRequestStatusHistory>();
}
