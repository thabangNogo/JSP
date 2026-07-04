using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Entities.MasterData;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Entities.Shared;

namespace JobSafetyPro.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Company> Companies { get; }
    IRepository<Plant> Plants { get; }
    IRepository<Department> Departments { get; }
    IUserRepository Users { get; }
    IEmployeeProfileRepository EmployeeProfiles { get; }
    IRepository<WorkDepartment> WorkDepartments { get; }
    IRepository<WorkLocation> WorkLocations { get; }
    IRepository<WorkSection> WorkSections { get; }
    IRepository<Role> Roles { get; }
    IJsaRepository JobSafetyAssessments { get; }
    IRepository<Hazard> Hazards { get; }
    IRepository<RiskLevel> RiskLevels { get; }
    IRepository<ControlMeasure> ControlMeasures { get; }
    IRepository<Incident> Incidents { get; }
    IRepository<NearMiss> NearMisses { get; }
    IRepository<Injury> Injuries { get; }
    IRepository<InjuryPhoto> InjuryPhotos { get; }
    IRepository<InjuryNotification> InjuryNotifications { get; }
    IRepository<StopUnsafeWork> StopUnsafeWorkReports { get; }
    IRepository<SafetyNotification> SafetyNotifications { get; }
    IRepository<UserDevice> UserDevices { get; }
    IRepository<CorrectiveAction> CorrectiveActions { get; }
    IRepository<Attachment> Attachments { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IAuditLogRepository AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void ClearChangeTracker();

    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetUsersInRolesAsync(
        IEnumerable<string> roleNames,
        Guid? companyId = null,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<User> Items, int TotalCount)> SearchEmployeesAsync(
        Guid companyId,
        string? search,
        string? department,
        string? occupation,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<User?> GetEmployeeByIdAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default);
}

public interface IJsaRepository : IRepository<JobSafetyAssessment>
{
    Task<JobSafetyAssessment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSafetyAssessment>> GetByStatusForUserAsync(
        JsaStatus status,
        Guid userId,
        string? department = null,
        string? location = null,
        string? section = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSafetyAssessment>> GetForReportsAsync(
        string? department = null,
        string? location = null,
        string? section = null,
        JsaStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all hazards/controls for a draft assessment (physical delete) so EF only inserts new rows.
    /// </summary>
    Task ReplaceChildrenPhysicallyAsync(Guid assessmentId, CancellationToken cancellationToken = default);

    Task<int> UpdateDraftHeaderAsync(
        Guid assessmentId,
        string title,
        string jobDescription,
        string department,
        string location,
        string section,
        Guid workLocationId,
        Guid workSectionId,
        int currentStep,
        string? workflowDataJson,
        string? signatureStoragePath,
        string? signOffName,
        string? signOffSurname,
        string? signOffCompanyNumber,
        string? signOffOccupation,
        JsaStatus status,
        DateTime lastSavedAt,
        Guid? createdByUserId,
        string modifiedBy,
        CancellationToken cancellationToken = default);
}

public interface IEmployeeProfileRepository : IRepository<EmployeeProfile>
{
    Task<EmployeeProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
