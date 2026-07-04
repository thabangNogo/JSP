using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.MasterData;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Safety;

public class JobSafetyAssessment : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid PlantId { get; set; }

    public Guid DepartmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string JobDescription { get; set; } = string.Empty;

    /// <summary>Job site department (free text), not the organization FK.</summary>
    public string Department { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public Guid? WorkLocationId { get; set; }

    public Guid? WorkSectionId { get; set; }

    public JsaStatus Status { get; set; } = JsaStatus.Draft;

    public int CurrentStep { get; set; }

    public string? WorkflowDataJson { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public string? SignOffName { get; set; }

    public string? SignOffSurname { get; set; }

    public string? SignOffCompanyNumber { get; set; }

    public string? SignOffOccupation { get; set; }

    public string? SignatureStoragePath { get; set; }

    public DateTime? LastSavedAt { get; set; }

    public int Version { get; set; } = 1;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? RejectedByUserId { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? RejectionReason { get; set; }

    public Company Company { get; set; } = null!;

    public Plant Plant { get; set; } = null!;

    public Department OrganizationDepartment { get; set; } = null!;

    public WorkLocation? WorkLocation { get; set; }

    public WorkSection? WorkSection { get; set; }

    public ICollection<Hazard> Hazards { get; set; } = new List<Hazard>();

    public ICollection<ControlMeasure> ControlMeasures { get; set; } = new List<ControlMeasure>();
}
