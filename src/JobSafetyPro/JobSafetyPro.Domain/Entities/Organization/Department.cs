using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.Safety;

namespace JobSafetyPro.Domain.Entities.Organization;

public class Department : BaseEntity
{
    public Guid PlantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Plant Plant { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<JobSafetyAssessment> JobSafetyAssessments { get; set; } = new List<JobSafetyAssessment>();

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public ICollection<NearMiss> NearMisses { get; set; } = new List<NearMiss>();
}
