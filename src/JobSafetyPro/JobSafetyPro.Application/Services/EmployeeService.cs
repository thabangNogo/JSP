using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Profile;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public interface IEmployeeService
{
    Task<PaginatedList<EmployeeListItemDto>> GetAllAsync(
        EmployeeSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<PaginatedList<EmployeeListItemDto>> SearchAsync(
        EmployeeSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<EmployeeDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmployeeStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);

    Task<EmployeeListItemDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
}

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IPasswordHasher _passwordHasher;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _passwordHasher = passwordHasher;
    }

    public Task<PaginatedList<EmployeeListItemDto>> GetAllAsync(
        EmployeeSearchQuery query,
        CancellationToken cancellationToken = default) =>
        SearchAsync(query, cancellationToken);

    public async Task<PaginatedList<EmployeeListItemDto>> SearchAsync(
        EmployeeSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var companyId = RequireCompanyId();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (users, total) = await _unitOfWork.Users.SearchEmployeesAsync(
            companyId,
            query.Search,
            query.Department,
            query.Occupation,
            page,
            pageSize,
            cancellationToken);

        var jsas = await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken);
        var nearMisses = await _unitOfWork.NearMisses.GetAllAsync(cancellationToken);

        var items = users.Select(u => MapListItem(u, jsas, nearMisses)).ToList();

        return new PaginatedList<EmployeeListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
        };
    }

    public async Task<EmployeeDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var companyId = RequireCompanyId();
        var user = await _unitOfWork.Users.GetEmployeeByIdAsync(id, companyId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);

        var jsas = (await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken))
            .Where(j => j.CreatedByUserId == id)
            .OrderByDescending(j => j.CreatedDate)
            .ToList();

        var nearMisses = (await _unitOfWork.NearMisses.GetAllAsync(cancellationToken))
            .Where(n => n.ReportedByUserId == id)
            .OrderByDescending(n => n.OccurredAt)
            .ToList();

        var actions = (await _unitOfWork.CorrectiveActions.GetAllAsync(cancellationToken))
            .Where(a => a.AssignedToUserId == id)
            .OrderByDescending(a => a.CreatedDate)
            .ToList();

        var profile = user.EmployeeProfile;
        var name = profile?.Name ?? user.FirstName;
        var surname = profile?.Surname ?? user.LastName;
        var department = profile?.WorkDepartment?.Name ?? string.Empty;
        var occupation = profile?.Occupation ?? string.Empty;
        var companyNumber = profile?.CompanyNumber ?? user.EmployeeNumber;

        var assessments = jsas.Select(j => new EmployeeAssessmentDto(
            j.Id,
            j.Title,
            j.Status.ToString(),
            j.CreatedDate,
            j.Status is JsaStatus.Approved or JsaStatus.Submitted ? j.ModifiedDate : null)).ToList();

        var nearMissDtos = nearMisses.Select(n => new EmployeeNearMissDto(
            n.Id,
            n.OccurredAt,
            n.Category.ToString(),
            n.Location,
            n.Status.ToString())).ToList();

        var actionDtos = actions.Select(a => new EmployeeCorrectiveActionDto(
            a.Id,
            a.Description,
            a.CreatedDate,
            a.DueDate,
            a.Status.ToString())).ToList();

        var timeline = BuildTimeline(jsas, nearMisses, actions);

        return new EmployeeDetailDto(
            user.Id,
            user.Email,
            user.EmployeeNumber,
            name,
            surname,
            department,
            occupation,
            companyNumber,
            user.CreatedDate,
            user.IsActive ? "Active" : "Inactive",
            user.IsActive,
            assessments,
            nearMissDtos,
            actionDtos,
            timeline);
    }

    public async Task<EmployeeStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var companyId = RequireCompanyId();
        var today = _dateTimeService.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var users = (await _unitOfWork.Users.GetAllAsync(cancellationToken))
            .Where(u => u.CompanyId == companyId)
            .ToList();
        var jsas = (await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken)).ToList();
        var nearMisses = (await _unitOfWork.NearMisses.GetAllAsync(cancellationToken)).ToList();

        var total = users.Count;
        var online = users.Count(u => u.IsActive);
        var assessmentsToday = jsas.Count(j =>
            j.Status is JsaStatus.Submitted or JsaStatus.Approved or JsaStatus.InReview
            && j.ModifiedDate?.Date == today);
        var nearMissesMonth = nearMisses.Count(n => n.CreatedDate >= monthStart);
        var completedJsas = jsas.Count(j =>
            j.Status is JsaStatus.Submitted or JsaStatus.Approved or JsaStatus.InReview);
        var avg = total > 0 ? Math.Round((double)completedJsas / total, 1) : 0;

        return new EmployeeStatsDto(total, online, assessmentsToday, nearMissesMonth, avg);
    }

    public async Task<EmployeeListItemDto> CreateEmployeeAsync(
        CreateEmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.Roles.Contains(AppRoles.Administrator))
        {
            throw new UnauthorizedAppException("Only administrators can create employees.");
        }

        var companyId = RequireCompanyId();
        var plantId = _currentUserService.PlantId;
        var orgDeptId = _currentUserService.Roles.Any()
            ? (await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId!.Value, cancellationToken))?.DepartmentId
            : null;

        var workDept = await _unitOfWork.WorkDepartments.GetByIdAsync(dto.WorkDepartmentId, cancellationToken)
            ?? throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["workDepartmentId"] = new[] { "Invalid work department." },
            });

        if (await _unitOfWork.Users.GetByEmailAsync(dto.Email.Trim(), cancellationToken) != null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["email"] = new[] { "Email is already registered." },
            });
        }

        var now = _dateTimeService.UtcNow;
        var user = new User
        {
            CompanyId = companyId,
            PlantId = plantId,
            DepartmentId = orgDeptId,
            Email = dto.Email.Trim(),
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            EmployeeNumber = dto.EmployeeNumber.Trim(),
            IsActive = true,
            CreatedDate = now,
            CreatedBy = _currentUserService.Email ?? "admin",
        };

        var roles = await _unitOfWork.Roles.FindAsync(r => dto.Roles.Contains(r.Name), cancellationToken);
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = user.Id, CreatedBy = "admin", CreatedDate = now });
        }

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.EmployeeProfiles.AddAsync(
            new EmployeeProfile
            {
                UserId = user.Id,
                WorkDepartmentId = workDept.Id,
                Name = dto.FirstName.Trim(),
                Surname = dto.LastName.Trim(),
                CompanyNumber = dto.CompanyNumber.Trim(),
                Occupation = dto.Occupation.Trim(),
                CreatedDate = now,
                CreatedBy = _currentUserService.Email ?? "admin",
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Users.GetEmployeeByIdAsync(user.Id, companyId, cancellationToken)
            ?? user;
        var jsas = await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken);
        var nearMisses = await _unitOfWork.NearMisses.GetAllAsync(cancellationToken);
        return MapListItem(created, jsas, nearMisses);
    }

    private Guid RequireCompanyId() =>
        _currentUserService.CompanyId
        ?? throw new UnauthorizedAppException("Company context is required.");

    private static EmployeeListItemDto MapListItem(
        User user,
        IReadOnlyList<JobSafetyPro.Domain.Entities.Safety.JobSafetyAssessment> jsas,
        IReadOnlyList<JobSafetyPro.Domain.Entities.Incidents.NearMiss> nearMisses)
    {
        var profile = user.EmployeeProfile;
        var assessmentsCompleted = jsas.Count(j =>
            j.CreatedByUserId == user.Id &&
            j.Status is JsaStatus.Submitted or JsaStatus.Approved or JsaStatus.InReview);
        var nearMissCount = nearMisses.Count(n => n.ReportedByUserId == user.Id);

        return new EmployeeListItemDto(
            user.Id,
            profile?.CompanyNumber ?? user.EmployeeNumber,
            profile?.Name ?? user.FirstName,
            profile?.Surname ?? user.LastName,
            profile?.WorkDepartment?.Name ?? string.Empty,
            profile?.Occupation ?? string.Empty,
            assessmentsCompleted,
            nearMissCount,
            user.IsActive ? "Active" : "Inactive",
            user.IsActive);
    }

    private static IReadOnlyList<EmployeeActivityDto> BuildTimeline(
        IReadOnlyList<JobSafetyPro.Domain.Entities.Safety.JobSafetyAssessment> jsas,
        IReadOnlyList<JobSafetyPro.Domain.Entities.Incidents.NearMiss> nearMisses,
        IReadOnlyList<JobSafetyPro.Domain.Entities.Incidents.CorrectiveAction> actions)
    {
        var events = new List<EmployeeActivityDto>();

        events.AddRange(jsas.Select(j => new EmployeeActivityDto(
            j.ModifiedDate ?? j.CreatedDate,
            "Assessment",
            $"{j.Title} — {j.Status}")));

        events.AddRange(nearMisses.Select(n => new EmployeeActivityDto(
            n.OccurredAt,
            "Near Miss",
            $"{n.Category} at {n.Location}")));

        events.AddRange(actions.Select(a => new EmployeeActivityDto(
            a.CreatedDate,
            "Corrective Action",
            a.Description)));

        return events.OrderByDescending(e => e.OccurredAt).Take(50).ToList();
    }
}
