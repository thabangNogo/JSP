using Microsoft.EntityFrameworkCore;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.MasterData;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Entities.Shared;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Companies = new Repository<Company>(context);
        Plants = new Repository<Plant>(context);
        Departments = new Repository<Department>(context);
        Users = new UserRepository(context);
        EmployeeProfiles = new EmployeeProfileRepository(context);
        WorkDepartments = new Repository<WorkDepartment>(context);
        WorkLocations = new Repository<WorkLocation>(context);
        WorkSections = new Repository<WorkSection>(context);
        Roles = new Repository<Role>(context);
        JobSafetyAssessments = new JsaRepository(context);
        Hazards = new Repository<Hazard>(context);
        RiskLevels = new Repository<RiskLevel>(context);
        ControlMeasures = new Repository<ControlMeasure>(context);
        Incidents = new Repository<Incident>(context);
        NearMisses = new Repository<NearMiss>(context);
        Injuries = new Repository<Injury>(context);
        InjuryPhotos = new Repository<InjuryPhoto>(context);
        InjuryNotifications = new Repository<InjuryNotification>(context);
        StopUnsafeWorkReports = new Repository<StopUnsafeWork>(context);
        SafetyNotifications = new Repository<SafetyNotification>(context);
        UserDevices = new Repository<UserDevice>(context);
        CorrectiveActions = new Repository<CorrectiveAction>(context);
        Attachments = new Repository<Attachment>(context);
        RefreshTokens = new Repository<RefreshToken>(context);
        AuditLogs = new AuditLogRepository(context);
    }

    public IRepository<Company> Companies { get; }
    public IRepository<Plant> Plants { get; }
    public IRepository<Department> Departments { get; }
    public IUserRepository Users { get; }
    public IEmployeeProfileRepository EmployeeProfiles { get; }
    public IRepository<WorkDepartment> WorkDepartments { get; }
    public IRepository<WorkLocation> WorkLocations { get; }
    public IRepository<WorkSection> WorkSections { get; }
    public IRepository<Role> Roles { get; }
    public IJsaRepository JobSafetyAssessments { get; }
    public IRepository<Hazard> Hazards { get; }
    public IRepository<RiskLevel> RiskLevels { get; }
    public IRepository<ControlMeasure> ControlMeasures { get; }
    public IRepository<Incident> Incidents { get; }
    public IRepository<NearMiss> NearMisses { get; }
    public IRepository<Injury> Injuries { get; }
    public IRepository<InjuryPhoto> InjuryPhotos { get; }
    public IRepository<InjuryNotification> InjuryNotifications { get; }
    public IRepository<StopUnsafeWork> StopUnsafeWorkReports { get; }
    public IRepository<SafetyNotification> SafetyNotifications { get; }
    public IRepository<UserDevice> UserDevices { get; }
    public IRepository<CorrectiveAction> CorrectiveActions { get; }
    public IRepository<Attachment> Attachments { get; }
    public IRepository<RefreshToken> RefreshTokens { get; }
    public IAuditLogRepository AuditLogs { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void ClearChangeTracker() => _context.ChangeTracker.Clear();

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public void Dispose() => _context.Dispose();
}
