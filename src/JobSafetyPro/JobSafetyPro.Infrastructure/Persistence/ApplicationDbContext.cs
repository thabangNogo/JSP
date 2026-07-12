using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.MasterData;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Entities.Ppe;
using JobSafetyPro.Domain.Entities.Shared;
using JobSafetyPro.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace JobSafetyPro.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor)
        : base(options)
    {
        _auditableEntityInterceptor = auditableEntityInterceptor;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<WorkDepartment> WorkDepartments => Set<WorkDepartment>();
    public DbSet<WorkLocation> WorkLocations => Set<WorkLocation>();
    public DbSet<WorkSection> WorkSections => Set<WorkSection>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<JobSafetyAssessment> JobSafetyAssessments => Set<JobSafetyAssessment>();
    public DbSet<Hazard> Hazards => Set<Hazard>();
    public DbSet<RiskLevel> RiskLevels => Set<RiskLevel>();
    public DbSet<ControlMeasure> ControlMeasures => Set<ControlMeasure>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<NearMiss> NearMisses => Set<NearMiss>();
    public DbSet<Injury> Injuries => Set<Injury>();
    public DbSet<InjuryPhoto> InjuryPhotos => Set<InjuryPhoto>();
    public DbSet<InjuryNotification> InjuryNotifications => Set<InjuryNotification>();

    public DbSet<StopUnsafeWork> StopUnsafeWorkReports => Set<StopUnsafeWork>();

    public DbSet<SafetyNotification> SafetyNotifications => Set<SafetyNotification>();

    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<CorrectiveAction> CorrectiveActions => Set<CorrectiveAction>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<PpeCatalogueItem> PpeCatalogueItems => Set<PpeCatalogueItem>();

    public DbSet<PpeRequest> PpeRequests => Set<PpeRequest>();

    public DbSet<PpeRequestStatusHistory> PpeRequestStatusHistories => Set<PpeRequestStatusHistory>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntityInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
