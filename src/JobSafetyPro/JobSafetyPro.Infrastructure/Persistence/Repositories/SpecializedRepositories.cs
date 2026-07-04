using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Entities.Shared;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobSafetyPro.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.TokenHash == tokenHash), cancellationToken);

    public async Task<IReadOnlyList<User>> GetUsersInRolesAsync(
        IEnumerable<string> roleNames,
        Guid? companyId = null,
        CancellationToken cancellationToken = default)
    {
        var names = roleNames.ToList();
        var query = DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => names.Contains(ur.Role.Name)));

        if (companyId.HasValue)
        {
            query = query.Where(u => u.CompanyId == companyId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> SearchEmployeesAsync(
        Guid companyId,
        string? search,
        string? department,
        string? occupation,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(u => u.EmployeeProfile)
            .ThenInclude(p => p!.WorkDepartment)
            .Where(u => u.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.EmployeeNumber.ToLower().Contains(term) ||
                (u.EmployeeProfile != null && (
                    u.EmployeeProfile.Name.ToLower().Contains(term) ||
                    u.EmployeeProfile.Surname.ToLower().Contains(term) ||
                    u.EmployeeProfile.CompanyNumber.ToLower().Contains(term))));
        }

        if (!string.IsNullOrWhiteSpace(department) && department != "All Departments")
        {
            var dept = department.Trim();
            query = query.Where(u =>
                u.EmployeeProfile != null &&
                u.EmployeeProfile.WorkDepartment.Name == dept);
        }

        if (!string.IsNullOrWhiteSpace(occupation) && occupation != "All Occupations")
        {
            var occ = occupation.Trim();
            query = query.Where(u =>
                u.EmployeeProfile != null &&
                u.EmployeeProfile.Occupation == occ);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<User?> GetEmployeeByIdAsync(
        Guid id,
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(u => u.EmployeeProfile)
            .ThenInclude(p => p!.WorkDepartment)
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId, cancellationToken);
}

public class JsaRepository : Repository<JobSafetyAssessment>, IJsaRepository
{
    public JsaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<JobSafetyAssessment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(j => j.Hazards)
            .Include(j => j.ControlMeasures)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<IReadOnlyList<JobSafetyAssessment>> GetByStatusForUserAsync(
        JsaStatus status,
        Guid userId,
        string? department = null,
        string? location = null,
        string? section = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(j => j.Status == status && j.CreatedByUserId == userId);

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(j => j.Department == department);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(j => j.Location == location);
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            query = query.Where(j => j.Section == section);
        }

        return await query
            .OrderByDescending(j => j.LastSavedAt ?? j.ModifiedDate ?? j.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobSafetyAssessment>> GetForReportsAsync(
        string? department = null,
        string? location = null,
        string? section = null,
        JsaStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(j => j.Department == department);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(j => j.Location == location);
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            query = query.Where(j => j.Section == section);
        }

        return await query
            .OrderByDescending(j => j.LastSavedAt ?? j.ModifiedDate ?? j.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceChildrenPhysicallyAsync(
        Guid assessmentId,
        CancellationToken cancellationToken = default)
    {
        // Physical delete avoids soft-delete UPDATE commands that cause concurrency conflicts.
        await Context.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ControlMeasures WHERE JobSafetyAssessmentId = {assessmentId}",
            cancellationToken);

        await Context.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM Hazards WHERE JobSafetyAssessmentId = {assessmentId}",
            cancellationToken);
    }

    public async Task<int> UpdateDraftHeaderAsync(
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
        CancellationToken cancellationToken = default)
    {
        return await Context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE JobSafetyAssessments
               SET Title = {title},
                   JobDescription = {jobDescription},
                   Department = {department},
                   Location = {location},
                   Section = {section},
                   WorkLocationId = {workLocationId},
                   WorkSectionId = {workSectionId},
                   CurrentStep = {currentStep},
                   WorkflowDataJson = {workflowDataJson},
                   SignatureStoragePath = {signatureStoragePath},
                   SignOffName = {signOffName},
                   SignOffSurname = {signOffSurname},
                   SignOffCompanyNumber = {signOffCompanyNumber},
                   SignOffOccupation = {signOffOccupation},
                   Status = {status.ToString()},
                   LastSavedAt = {lastSavedAt},
                   ModifiedDate = {lastSavedAt},
                   ModifiedBy = {modifiedBy},
                   CreatedByUserId = COALESCE(CreatedByUserId, {createdByUserId})
               WHERE Id = {assessmentId} AND IsDeleted = 0",
            cancellationToken);
    }
}

public class EmployeeProfileRepository : Repository<EmployeeProfile>, IEmployeeProfileRepository
{
    public EmployeeProfileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<EmployeeProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(p => p.WorkDepartment)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
}

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(auditLog, cancellationToken);
}
