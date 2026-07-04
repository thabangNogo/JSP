using JSP.Domain.Common;
using JSP.Domain.Entities.Organization;

namespace JSP.Domain.Entities.Safety;

public enum JsaStatus
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Expired = 3,
    Archived = 4
}

public class JobSafetyAssessment : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid PlantId { get; set; }

    public Guid DepartmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string JobDescription { get; set; } = string.Empty;

    public JsaStatus Status { get; set; } = JsaStatus.Draft;

    public int Version { get; set; } = 1;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Company Company { get; set; } = null!;

    public Plant Plant { get; set; } = null!;

    public Department Department { get; set; } = null!;

    public ICollection<AssessmentHazard> AssessmentHazards { get; set; } = new List<AssessmentHazard>();
}
