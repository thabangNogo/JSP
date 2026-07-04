using JSP.Domain.Entities.Identity;
using JSP.Domain.Entities.Incidents;
using JSP.Domain.Entities.Organization;
using JSP.Domain.Entities.Safety;
using JSP.Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;

namespace JSP.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Organization
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<Department> Departments => Set<Department>();

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Safety
    public DbSet<JobSafetyAssessment> JobSafetyAssessments => Set<JobSafetyAssessment>();
    public DbSet<HazardCategory> HazardCategories => Set<HazardCategory>();
    public DbSet<Hazard> Hazards => Set<Hazard>();
    public DbSet<RiskLevel> RiskLevels => Set<RiskLevel>();
    public DbSet<AssessmentHazard> AssessmentHazards => Set<AssessmentHazard>();
    public DbSet<ControlMeasure> ControlMeasures => Set<ControlMeasure>();
    public DbSet<HierarchyOfControl> HierarchyOfControls => Set<HierarchyOfControl>();
    public DbSet<AssessmentControl> AssessmentControls => Set<AssessmentControl>();
    public DbSet<GoldenRule> GoldenRules => Set<GoldenRule>();

    // Incidents
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<NearMiss> NearMisses => Set<NearMiss>();
    public DbSet<CorrectiveAction> CorrectiveActions => Set<CorrectiveAction>();

    // Shared
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Signature> Signatures => Set<Signature>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
