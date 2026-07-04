using FluentValidation;
using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.Shared;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public interface IInjuryFreeDaysService
{
    Task<int> GetCurrentInjuryFreeDaysAsync(CancellationToken cancellationToken = default);
}

public interface IInjuryDashboardService
{
    Task<InjuryDashboardKpiDto> GetDashboardKpisAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InjuryRegisterRowDto>> GetInjuryRegisterAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InjuryTrendPointDto>> GetInjuryTrendsAsync(CancellationToken cancellationToken = default);
    Task<InjuryFrequencyRateDto> GetInjuryFrequencyRateAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NamedCountDto>> GetInjuriesByDepartmentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NamedCountDto>> GetInjuriesByLocationAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NamedCountDto>> GetInjuriesBySectionAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NamedCountDto>> GetInjuriesByBodyPartAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InjuryFreeDaysHistoryPointDto>> GetInjuryFreeDaysHistoryAsync(CancellationToken cancellationToken = default);
}

public interface IInjuryService
{
    Task<IReadOnlyList<InjuryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InjuryDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InjuryDetailDto> CreateAsync(CreateInjuryDto dto, CancellationToken cancellationToken = default);
    Task<InjuryDetailDto> UpdateAsync(Guid id, UpdateInjuryDto dto, CancellationToken cancellationToken = default);
}

public class InjuryFreeDaysService : IInjuryFreeDaysService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTimeService;

    public InjuryFreeDaysService(IUnitOfWork unitOfWork, IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _dateTimeService = dateTimeService;
    }

    public async Task<int> GetCurrentInjuryFreeDaysAsync(CancellationToken cancellationToken = default)
    {
        var injuries = await _unitOfWork.Injuries.GetAllAsync(cancellationToken);
        var latestSubmitted = injuries
            .Where(i => i.IsSubmitted)
            .OrderByDescending(i => i.SubmittedAt)
            .FirstOrDefault();

        if (latestSubmitted == null)
        {
            return 0;
        }

        var today = _dateTimeService.UtcNow.Date;
        var resetDate = latestSubmitted.InjuryFreeDaysResetDate.Date;
        var days = (today - resetDate).Days;
        return Math.Max(0, days);
    }
}

public class InjuryDashboardService : IInjuryDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInjuryFreeDaysService _injuryFreeDaysService;
    private readonly IDateTimeService _dateTimeService;

    public InjuryDashboardService(
        IUnitOfWork unitOfWork,
        IInjuryFreeDaysService injuryFreeDaysService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _injuryFreeDaysService = injuryFreeDaysService;
        _dateTimeService = dateTimeService;
    }

    public async Task<InjuryDashboardKpiDto> GetDashboardKpisAsync(CancellationToken cancellationToken = default)
    {
        var today = _dateTimeService.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var injuries = (await _unitOfWork.Injuries.GetAllAsync(cancellationToken)).ToList();
        var nearMisses = (await _unitOfWork.NearMisses.GetAllAsync(cancellationToken)).ToList();
        var actions = (await _unitOfWork.CorrectiveActions.GetAllAsync(cancellationToken)).ToList();
        var jsas = (await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken)).ToList();
        var users = (await _unitOfWork.Users.GetAllAsync(cancellationToken)).ToList();

        var injuryFreeDays = await _injuryFreeDaysService.GetCurrentInjuryFreeDaysAsync(cancellationToken);
        var openNearMisses = nearMisses.Count(n =>
            n.Status is NearMissStatus.Submitted or NearMissStatus.UnderInvestigation);
        var openActions = actions.Count(a =>
            a.Status is CorrectiveActionStatus.Open or CorrectiveActionStatus.InProgress or CorrectiveActionStatus.Overdue);
        var submittedToday = jsas.Count(j =>
            j.Status is JsaStatus.Submitted or JsaStatus.Approved or JsaStatus.InReview
            && j.ModifiedDate?.Date == today);
        var monthNearMisses = nearMisses.Count(n => n.CreatedDate >= monthStart);

        return new InjuryDashboardKpiDto(
            injuryFreeDays,
            openNearMisses,
            openActions,
            submittedToday,
            users.Count,
            monthNearMisses,
            injuries.Count(i => i.InjuryType == InjuryType.LostTimeInjury),
            injuries.Count(i => i.InjuryType == InjuryType.MedicalTreatmentInjury),
            injuries.Count(i => i.InjuryType == InjuryType.FirstAidInjury));
    }

    public async Task<IReadOnlyList<InjuryRegisterRowDto>> GetInjuryRegisterAsync(
        CancellationToken cancellationToken = default)
    {
        var injuries = await _unitOfWork.Injuries.GetAllAsync(cancellationToken);
        return injuries
            .OrderByDescending(i => i.InjuryOccurredAt)
            .Select(i => new InjuryRegisterRowDto(
                i.Id,
                i.InjuryOccurredAt,
                i.EmployeeName,
                i.Department,
                i.Location,
                i.Section,
                i.InjuryType,
                i.BodyPartInjured,
                i.Status))
            .ToList();
    }

    public async Task<IReadOnlyList<InjuryTrendPointDto>> GetInjuryTrendsAsync(
        CancellationToken cancellationToken = default)
    {
        var injuries = (await _unitOfWork.Injuries.GetAllAsync(cancellationToken)).ToList();
        var monthStart = new DateTime(_dateTimeService.UtcNow.Year, _dateTimeService.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return Enumerable.Range(0, 12)
            .Select(i =>
            {
                var start = monthStart.AddMonths(-11 + i);
                var end = start.AddMonths(1);
                var count = injuries.Count(x => x.InjuryOccurredAt >= start && x.InjuryOccurredAt < end);
                return new InjuryTrendPointDto(start.ToString("MMM yyyy"), count);
            })
            .ToList();
    }

    public async Task<InjuryFrequencyRateDto> GetInjuryFrequencyRateAsync(
        CancellationToken cancellationToken = default)
    {
        var injuries = (await _unitOfWork.Injuries.GetAllAsync(cancellationToken)).ToList();
        var users = (await _unitOfWork.Users.GetAllAsync(cancellationToken)).ToList();
        var employeeCount = Math.Max(users.Count, 1);
        var yearStart = new DateTime(_dateTimeService.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearInjuries = injuries.Count(i => i.InjuryOccurredAt >= yearStart);
        var rate = Math.Round((double)yearInjuries / employeeCount * 100, 2);
        return new InjuryFrequencyRateDto(rate, yearInjuries, employeeCount, _dateTimeService.UtcNow.Year.ToString());
    }

    public async Task<IReadOnlyList<NamedCountDto>> GetInjuriesByDepartmentAsync(
        CancellationToken cancellationToken = default) =>
        GroupInjuries(await _unitOfWork.Injuries.GetAllAsync(cancellationToken), i => i.Department);

    public async Task<IReadOnlyList<NamedCountDto>> GetInjuriesByLocationAsync(
        CancellationToken cancellationToken = default) =>
        GroupInjuries(await _unitOfWork.Injuries.GetAllAsync(cancellationToken), i => i.Location);

    public async Task<IReadOnlyList<NamedCountDto>> GetInjuriesBySectionAsync(
        CancellationToken cancellationToken = default) =>
        GroupInjuries(await _unitOfWork.Injuries.GetAllAsync(cancellationToken), i => i.Section);

    public async Task<IReadOnlyList<NamedCountDto>> GetInjuriesByBodyPartAsync(
        CancellationToken cancellationToken = default) =>
        GroupInjuries(await _unitOfWork.Injuries.GetAllAsync(cancellationToken), i => i.BodyPartInjured.ToString());

    public async Task<IReadOnlyList<InjuryFreeDaysHistoryPointDto>> GetInjuryFreeDaysHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var injuries = (await _unitOfWork.Injuries.GetAllAsync(cancellationToken))
            .Where(i => i.IsSubmitted)
            .OrderBy(i => i.SubmittedAt)
            .ToList();

        var today = _dateTimeService.UtcNow.Date;
        var history = new List<InjuryFreeDaysHistoryPointDto>();

        if (injuries.Count == 0)
        {
            history.Add(new InjuryFreeDaysHistoryPointDto(today.ToString("MMM yyyy"), 0));
            return history;
        }

        for (var i = 0; i < injuries.Count; i++)
        {
            var injury = injuries[i];
            var nextReset = i < injuries.Count - 1
                ? injuries[i + 1].InjuryFreeDaysResetDate.Date
                : today;
            var days = (nextReset - injury.InjuryFreeDaysResetDate.Date).Days;
            history.Add(new InjuryFreeDaysHistoryPointDto(
                injury.InjuryOccurredAt.ToString("MMM yyyy"),
                Math.Max(0, days)));
        }

        return history;
    }

    private static IReadOnlyList<NamedCountDto> GroupInjuries(
        IEnumerable<Injury> injuries,
        Func<Injury, string> selector) =>
        injuries.GroupBy(selector)
            .Select(g => new NamedCountDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
}

public class InjuryService : IInjuryService
{
    private static readonly string[] CaptureRoles =
    {
        AppRoles.SafetyManager,
        AppRoles.SafetyOfficer,
        AppRoles.Administrator,
        AppRoles.HseManager,
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmployeeProfileService _employeeProfileService;
    private readonly ISafetyNotificationDispatcher _notifications;
    private readonly IDateTimeService _dateTimeService;
    private readonly IValidator<CreateInjuryDto> _createValidator;
    private readonly IValidator<UpdateInjuryDto> _updateValidator;

    public InjuryService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmployeeProfileService employeeProfileService,
        ISafetyNotificationDispatcher notifications,
        IDateTimeService dateTimeService,
        IValidator<CreateInjuryDto> createValidator,
        IValidator<UpdateInjuryDto> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _employeeProfileService = employeeProfileService;
        _notifications = notifications;
        _dateTimeService = dateTimeService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<InjuryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanCapture();
        var injuries = await _unitOfWork.Injuries.GetAllAsync(cancellationToken);
        return injuries
            .OrderByDescending(i => i.InjuryOccurredAt)
            .Select(MapSummary)
            .ToList();
    }

    public async Task<InjuryDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureCanCapture();
        var injury = await _unitOfWork.Injuries.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Injury), id);
        return await MapDetailAsync(injury, cancellationToken);
    }

    public async Task<InjuryDetailDto> CreateAsync(
        CreateInjuryDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureCanCapture();
        await _createValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        if (!user.DepartmentId.HasValue || !user.PlantId.HasValue)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["departmentId"] = new[] { "Your account is missing plant or department assignment. Contact your administrator." },
            });
        }

        var profile = await _employeeProfileService.GetMyProfileAsync(cancellationToken);
        var now = _dateTimeService.UtcNow;
        var resetDate = dto.InjuryOccurredAt.Date;

        var entity = new Injury
        {
            CompanyId = user.CompanyId,
            PlantId = user.PlantId.Value,
            DepartmentId = user.DepartmentId.Value,
            EmployeeUserId = dto.EmployeeUserId,
            EmployeeName = dto.EmployeeName.Trim(),
            Department = dto.Department.Trim(),
            Location = dto.Location.Trim(),
            Section = dto.Section.Trim(),
            InjuryOccurredAt = dto.InjuryOccurredAt,
            InjuryFreeDaysResetDate = resetDate,
            SubmittedAt = now,
            IsSubmitted = true,
            InjuryType = dto.InjuryType,
            BodyPartInjured = dto.BodyPartInjured,
            IncidentDescription = dto.IncidentDescription.Trim(),
            ImmediateActionTaken = dto.ImmediateActionTaken?.Trim(),
            RootCause = dto.RootCause?.Trim(),
            CorrectiveAction = dto.CorrectiveAction?.Trim(),
            LostTimeDays = dto.LostTimeDays,
            Witnesses = dto.Witnesses?.Trim(),
            Status = dto.Status,
            CapturedByUserId = userId,
            CreatedDate = now,
            CreatedBy = _currentUserService.Email ?? userId.ToString(),
        };

        await _unitOfWork.Injuries.AddAsync(entity, cancellationToken);
        await SavePhotosAsync(entity.Id, dto.PhotoStoragePaths, cancellationToken);
        await SaveAttachmentsAsync(entity.Id, dto.AttachmentStoragePaths, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await NotifyInjurySubmittedAsync(entity, cancellationToken);

        return await MapDetailAsync(entity, cancellationToken);
    }

    public async Task<InjuryDetailDto> UpdateAsync(
        Guid id,
        UpdateInjuryDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureCanCapture();
        await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var injury = await _unitOfWork.Injuries.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Injury), id);

        injury.EmployeeName = dto.EmployeeName.Trim();
        injury.Department = dto.Department.Trim();
        injury.Location = dto.Location.Trim();
        injury.Section = dto.Section.Trim();
        injury.InjuryType = dto.InjuryType;
        injury.BodyPartInjured = dto.BodyPartInjured;
        injury.IncidentDescription = dto.IncidentDescription.Trim();
        injury.ImmediateActionTaken = dto.ImmediateActionTaken?.Trim();
        injury.RootCause = dto.RootCause?.Trim();
        injury.CorrectiveAction = dto.CorrectiveAction?.Trim();
        injury.LostTimeDays = dto.LostTimeDays;
        injury.Witnesses = dto.Witnesses?.Trim();
        injury.Status = dto.Status;
        injury.ModifiedDate = _dateTimeService.UtcNow;
        injury.ModifiedBy = _currentUserService.Email ?? injury.ModifiedBy;

        _unitOfWork.Injuries.Update(injury);

        if (dto.PhotoStoragePaths is { Count: > 0 })
        {
            await SavePhotosAsync(injury.Id, dto.PhotoStoragePaths, cancellationToken);
        }

        if (dto.AttachmentStoragePaths is { Count: > 0 })
        {
            await SaveAttachmentsAsync(injury.Id, dto.AttachmentStoragePaths, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapDetailAsync(injury, cancellationToken);
    }

    private void EnsureCanCapture()
    {
        if (!_currentUserService.Roles.Any(r => CaptureRoles.Contains(r)))
        {
            throw new UnauthorizedAppException("Only Safety Manager or Safety Officer may manage injuries.");
        }
    }

    private async Task NotifyInjurySubmittedAsync(Injury entity, CancellationToken cancellationToken)
    {
        var title = "Injury Reported";
        var message =
            $"Injury reported for {entity.EmployeeName} at {entity.Location} - {entity.Section}. Type: {entity.InjuryType}.";

        await _notifications.NotifyRolesAsync(
            new[] { AppRoles.SafetyManager, AppRoles.HseManager, AppRoles.Supervisor },
            title,
            message,
            NotificationPriority.Critical,
            WorkflowNotificationType.InjuryCaptured,
            relatedEntityType: RelatedEntityTypes.Injury,
            relatedEntityId: entity.Id,
            cancellationToken);

        var notifiedUsers = await _unitOfWork.Users.GetUsersInRolesAsync(
            new[] { AppRoles.SafetyManager, AppRoles.HseManager, AppRoles.Supervisor },
            entity.CompanyId,
            cancellationToken);

        foreach (var user in notifiedUsers)
        {
            await _unitOfWork.InjuryNotifications.AddAsync(
                new InjuryNotification
                {
                    InjuryId = entity.Id,
                    UserId = user.Id,
                    Title = title,
                    Message = message,
                    Priority = NotificationPriority.Critical,
                    SentAt = _dateTimeService.UtcNow,
                    CreatedDate = _dateTimeService.UtcNow,
                    CreatedBy = _currentUserService.Email ?? "system",
                },
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SavePhotosAsync(
        Guid injuryId,
        IReadOnlyList<string>? paths,
        CancellationToken cancellationToken)
    {
        if (paths == null || paths.Count == 0) return;

        foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            await _unitOfWork.InjuryPhotos.AddAsync(
                new InjuryPhoto
                {
                    InjuryId = injuryId,
                    FileName = Path.GetFileName(path),
                    ContentType = "image/jpeg",
                    StoragePath = path,
                    FileSizeBytes = 0,
                    CreatedDate = _dateTimeService.UtcNow,
                    CreatedBy = _currentUserService.Email ?? "mobile",
                },
                cancellationToken);
        }
    }

    private async Task SaveAttachmentsAsync(
        Guid injuryId,
        IReadOnlyList<string>? paths,
        CancellationToken cancellationToken)
    {
        if (paths == null || paths.Count == 0) return;

        foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            await _unitOfWork.Attachments.AddAsync(
                new Attachment
                {
                    RelatedEntityType = RelatedEntityTypes.Injury,
                    RelatedEntityId = injuryId,
                    FileName = Path.GetFileName(path),
                    ContentType = "application/octet-stream",
                    StoragePath = path,
                    FileSizeBytes = 0,
                    CreatedDate = _dateTimeService.UtcNow,
                    CreatedBy = _currentUserService.Email ?? "mobile",
                },
                cancellationToken);
        }
    }

    private async Task<InjuryDetailDto> MapDetailAsync(Injury injury, CancellationToken cancellationToken)
    {
        var photos = await _unitOfWork.InjuryPhotos.GetAllAsync(cancellationToken);
        var attachments = await _unitOfWork.Attachments.GetAllAsync(cancellationToken);

        return new InjuryDetailDto(
            injury.Id,
            injury.CompanyId,
            injury.PlantId,
            injury.DepartmentId,
            injury.EmployeeUserId,
            injury.EmployeeName,
            injury.Department,
            injury.Location,
            injury.Section,
            injury.InjuryOccurredAt,
            injury.InjuryType,
            injury.BodyPartInjured,
            injury.IncidentDescription,
            injury.ImmediateActionTaken,
            injury.RootCause,
            injury.CorrectiveAction,
            injury.LostTimeDays,
            injury.Witnesses,
            injury.Status,
            injury.SubmittedAt,
            injury.CapturedByUserId,
            photos.Where(p => p.InjuryId == injury.Id).Select(p => p.StoragePath).ToList(),
            attachments.Where(a => a.RelatedEntityId == injury.Id && a.RelatedEntityType == RelatedEntityTypes.Injury)
                .Select(a => a.StoragePath).ToList(),
            injury.CreatedDate);
    }

    private static InjuryDto MapSummary(Injury injury) =>
        new(
            injury.Id,
            injury.EmployeeName,
            injury.Department,
            injury.Location,
            injury.Section,
            injury.InjuryOccurredAt,
            injury.InjuryType,
            injury.BodyPartInjured,
            injury.Status,
            injury.SubmittedAt,
            injury.CreatedDate);
}
