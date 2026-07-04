using JSP.Domain.Common;
using JSP.Domain.Entities.Identity;
using JSP.Domain.Entities.Incidents;
using JSP.Domain.Entities.Safety;

namespace JSP.Domain.Entities.Organization;

public class Plant : BaseEntity
{
    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string TimeZone { get; set; } = "UTC";

    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;

    public ICollection<Department> Departments { get; set; } = new List<Department>();

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<JobSafetyAssessment> JobSafetyAssessments { get; set; } = new List<JobSafetyAssessment>();

    public ICollection<GoldenRule> GoldenRules { get; set; } = new List<GoldenRule>();

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public ICollection<NearMiss> NearMisses { get; set; } = new List<NearMiss>();
}
