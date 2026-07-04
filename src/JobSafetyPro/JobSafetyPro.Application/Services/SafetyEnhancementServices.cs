using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.DTOs.Workflow;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.Shared;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public interface IStopUnsafeWorkService
{
    Task<StopUnsafeWorkDto> CreateAsync(CreateStopUnsafeWorkDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StopUnsafeWorkDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StopUnsafeWorkDto> AcknowledgeAsync(Guid id, AcknowledgeStopUnsafeWorkDto dto, CancellationToken cancellationToken = default);
    Task<StopUnsafeWorkDto> MarkWorkStoppedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StopUnsafeWorkDto> ResolveAsync(Guid id, ResolveStopUnsafeWorkDto dto, CancellationToken cancellationToken = default);
    Task<StopUnsafeWorkDto> VerifyAndCloseAsync(Guid id, VerifyStopUnsafeWorkDto dto, CancellationToken cancellationToken = default);
}

public interface ISafetyNotificationService
{
    Task<IReadOnlyList<SafetyNotificationDto>> GetMyNotificationsAsync(CancellationToken cancellationToken = default);
    Task<NotificationSummaryDto> GetUnreadSummaryAsync(CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task RegisterDeviceAsync(RegisterDeviceDto dto, CancellationToken cancellationToken = default);
}

public interface ISafetyKpiService
{
    Task<EmployeeSafetyKpiDto> GetEmployeeKpisAsync(CancellationToken cancellationToken = default);
    Task<ManagerSafetyKpiDto> GetManagerKpisAsync(CancellationToken cancellationToken = default);
}

public interface ISafetyEscalationService
{
    Task ProcessOverdueCorrectiveActionsAsync(CancellationToken cancellationToken = default);
}

public class StopUnsafeWorkService : IStopUnsafeWorkService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmployeeProfileService _employeeProfileService;
    private readonly ISafetyNotificationDispatcher _notifications;
    private readonly IDateTimeService _dateTimeService;
    private readonly IAuditService _auditService;

    public StopUnsafeWorkService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmployeeProfileService employeeProfileService,
        ISafetyNotificationDispatcher notifications,
        IDateTimeService dateTimeService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _employeeProfileService = employeeProfileService;
        _notifications = notifications;
        _dateTimeService = dateTimeService;
        _auditService = auditService;
    }

    public async Task<StopUnsafeWorkDto> CreateAsync(
        CreateStopUnsafeWorkDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var profile = await _employeeProfileService.GetMyProfileAsync(cancellationToken);

        var entity = new StopUnsafeWork
        {
            ReportedByUserId = userId,
            Department = profile?.WorkDepartmentName ?? string.Empty,
            Location = dto.Location.Trim(),
            Section = dto.Section.Trim(),
            Category = dto.Category,
            Description = dto.Description.Trim(),
            ImmediateRisk = dto.ImmediateRisk,
            ActionsTaken = dto.ActionsTaken?.Trim(),
            Status = StopUnsafeWorkStatus.Submitted,
            CreatedDate = _dateTimeService.UtcNow,
            CreatedBy = _currentUserService.Email ?? userId.ToString(),
        };

        await _unitOfWork.StopUnsafeWorkReports.AddAsync(entity, cancellationToken);
        await SavePhotosAsync(entity.Id, dto.PhotoStoragePaths, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var title = "Stop Unsafe Work Reported";
        var message =
            $"Critical: Unsafe work reported at {entity.Location} - {entity.Section}. Category: {entity.Category}.";

        await _notifications.NotifyRolesAsync(
            new[] { AppRoles.SafetyOfficer, AppRoles.Supervisor, AppRoles.SafetyManager, AppRoles.HseManager },
            title,
            message,
            NotificationPriority.Critical,
            WorkflowNotificationType.UnsafeWorkReported,
            relatedEntityType: RelatedEntityTypes.StopUnsafeWork,
            relatedEntityId: entity.Id,
            cancellationToken);

        return MapStopUnsafeWork(entity);
    }

    public async Task<IReadOnlyList<StopUnsafeWorkDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.StopUnsafeWorkReports.GetAllAsync(cancellationToken);
        return items.Select(MapStopUnsafeWork).ToList();
    }

    public async Task<StopUnsafeWorkDto> AcknowledgeAsync(
        Guid id,
        AcknowledgeStopUnsafeWorkDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSupervisor();
        var entity = await GetEntity(id, cancellationToken);
        if (entity.Status != StopUnsafeWorkStatus.Submitted)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Only submitted reports can be acknowledged." },
            });
        }

        var userId = _currentUserService.UserId!.Value;
        var now = _dateTimeService.UtcNow;
        entity.Status = StopUnsafeWorkStatus.Acknowledged;
        entity.AcknowledgedByUserId = userId;
        entity.AcknowledgedAt = now;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            entity.ActionsTaken = dto.Notes.Trim();
        }

        await SaveTransition(entity, "Acknowledge", cancellationToken);
        return MapStopUnsafeWork(entity);
    }

    public async Task<StopUnsafeWorkDto> MarkWorkStoppedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureSupervisor();
        var entity = await GetEntity(id, cancellationToken);
        if (entity.Status != StopUnsafeWorkStatus.Acknowledged)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Report must be acknowledged before marking work stopped." },
            });
        }

        entity.Status = StopUnsafeWorkStatus.WorkStopped;
        entity.WorkStoppedAt = _dateTimeService.UtcNow;
        await SaveTransition(entity, "WorkStopped", cancellationToken);
        return MapStopUnsafeWork(entity);
    }

    public async Task<StopUnsafeWorkDto> ResolveAsync(
        Guid id,
        ResolveStopUnsafeWorkDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSupervisor();
        var entity = await GetEntity(id, cancellationToken);
        if (entity.Status is not (StopUnsafeWorkStatus.WorkStopped or StopUnsafeWorkStatus.Acknowledged))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Report must be acknowledged or work stopped before resolving." },
            });
        }

        var userId = _currentUserService.UserId!.Value;
        entity.Status = StopUnsafeWorkStatus.Resolved;
        entity.ResolvedAt = _dateTimeService.UtcNow;
        entity.ResolvedByUserId = userId;
        entity.CorrectiveActionNotes = dto.CorrectiveActionNotes.Trim();
        if (!string.IsNullOrWhiteSpace(dto.ActionsTaken))
        {
            entity.ActionsTaken = dto.ActionsTaken.Trim();
        }

        await SaveTransition(entity, "Resolve", cancellationToken);
        return MapStopUnsafeWork(entity);
    }

    public async Task<StopUnsafeWorkDto> VerifyAndCloseAsync(
        Guid id,
        VerifyStopUnsafeWorkDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();
        var entity = await GetEntity(id, cancellationToken);
        if (entity.Status != StopUnsafeWorkStatus.Resolved)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Report must be resolved before closing." },
            });
        }

        var userId = _currentUserService.UserId!.Value;
        var now = _dateTimeService.UtcNow;
        entity.Status = StopUnsafeWorkStatus.Closed;
        entity.VerifiedAt = now;
        entity.VerifiedByUserId = userId;
        entity.ClosedAt = now;
        await SaveTransition(entity, "Close", cancellationToken);
        return MapStopUnsafeWork(entity);
    }

    private async Task<StopUnsafeWork> GetEntity(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.StopUnsafeWorkReports.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(StopUnsafeWork), id);
    }

    private async Task SaveTransition(StopUnsafeWork entity, string action, CancellationToken cancellationToken)
    {
        entity.ModifiedDate = _dateTimeService.UtcNow;
        entity.ModifiedBy = _currentUserService.Email ?? entity.ModifiedBy;
        _unitOfWork.StopUnsafeWorkReports.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(action, RelatedEntityTypes.StopUnsafeWork, entity.Id, cancellationToken: cancellationToken);
    }

    private void EnsureSupervisor()
    {
        if (!_currentUserService.Roles.Any(r =>
                r is AppRoles.Administrator or AppRoles.HseManager or AppRoles.Supervisor))
        {
            throw new UnauthorizedAppException("Supervisor access required.");
        }
    }

    private void EnsureSafetyLead()
    {
        if (!_currentUserService.Roles.Any(r =>
                r is AppRoles.Administrator or AppRoles.HseManager or AppRoles.SafetyManager
                    or AppRoles.SafetyOfficer))
        {
            throw new UnauthorizedAppException("Safety lead access required.");
        }
    }

    private async Task SavePhotosAsync(
        Guid entityId,
        IReadOnlyList<string>? paths,
        CancellationToken cancellationToken)
    {
        if (paths == null || paths.Count == 0) return;

        foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            await _unitOfWork.Attachments.AddAsync(
                new Attachment
                {
                    RelatedEntityType = RelatedEntityTypes.StopUnsafeWork,
                    RelatedEntityId = entityId,
                    FileName = System.IO.Path.GetFileName(path),
                    ContentType = "image/jpeg",
                    StoragePath = path,
                    FileSizeBytes = 0,
                    CreatedDate = _dateTimeService.UtcNow,
                    CreatedBy = _currentUserService.Email ?? "mobile",
                },
                cancellationToken);
        }
    }

    private static StopUnsafeWorkDto MapStopUnsafeWork(StopUnsafeWork e) =>
        new(
            e.Id,
            e.ReportedByUserId,
            e.Department,
            e.Location,
            e.Section,
            e.Category,
            e.Description,
            e.ImmediateRisk,
            e.ActionsTaken,
            e.Status,
            e.CreatedDate);
}

public class SafetyNotificationService : ISafetyNotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public SafetyNotificationService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task<IReadOnlyList<SafetyNotificationDto>> GetMyNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var all = await _unitOfWork.SafetyNotifications.GetAllAsync(cancellationToken);
        return all
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .Select(Map)
            .ToList();
    }

    public async Task<NotificationSummaryDto> GetUnreadSummaryAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var all = await _unitOfWork.SafetyNotifications.GetAllAsync(cancellationToken);
        var unread = all.Count(n => n.UserId == userId && !n.IsRead);
        return new NotificationSummaryDto(unread);
    }

    public async Task MarkReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var notification = await _unitOfWork.SafetyNotifications.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(SafetyNotification), id);
        if (notification.UserId != userId) throw new UnauthorizedAppException("Not your notification.");
        notification.IsRead = true;
        notification.ReadAt = _dateTimeService.UtcNow;
        _unitOfWork.SafetyNotifications.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterDeviceAsync(RegisterDeviceDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var all = await _unitOfWork.UserDevices.GetAllAsync(cancellationToken);
        var existing = all.FirstOrDefault(d => d.FcmToken == dto.FcmToken);
        if (existing != null)
        {
            existing.LastSeenAt = _dateTimeService.UtcNow;
            existing.Platform = dto.Platform;
            _unitOfWork.UserDevices.Update(existing);
        }
        else
        {
            await _unitOfWork.UserDevices.AddAsync(
                new UserDevice
                {
                    UserId = userId,
                    FcmToken = dto.FcmToken.Trim(),
                    Platform = dto.Platform.Trim(),
                    LastSeenAt = _dateTimeService.UtcNow,
                    CreatedDate = _dateTimeService.UtcNow,
                    CreatedBy = _currentUserService.Email ?? userId.ToString(),
                },
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static SafetyNotificationDto Map(SafetyNotification n) =>
        new(n.Id, n.Title, n.Message, n.Priority, n.NotificationType, n.RelatedEntityType, n.RelatedEntityId, n.IsRead, n.CreatedDate);
}

public class SafetyKpiService : ISafetyKpiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IInjuryFreeDaysService _injuryFreeDaysService;

    public SafetyKpiService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IInjuryFreeDaysService injuryFreeDaysService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _injuryFreeDaysService = injuryFreeDaysService;
    }

    public async Task<EmployeeSafetyKpiDto> GetEmployeeKpisAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAppException("User is not authenticated.");
        var monthStart = new DateTime(_dateTimeService.UtcNow.Year, _dateTimeService.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var nearMisses = await _unitOfWork.NearMisses.GetAllAsync(cancellationToken);
        var myNearMisses = nearMisses.Count(n =>
            n.ReportedByUserId == userId && n.CreatedDate >= monthStart);

        var jsas = await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken);
        var myJsas = jsas.Where(j => j.CreatedByUserId == userId).ToList();
        var drafts = myJsas.Count(j => j.Status == JsaStatus.Draft);
        var submitted = myJsas.Count(j =>
            j.Status is JsaStatus.Submitted or JsaStatus.InReview);
        var approved = myJsas.Count(j => j.Status == JsaStatus.Approved);

        var score = (myNearMisses * 10) + (submitted * 15) + (approved * 20) + (drafts * 5);
        var injuryFreeDays = await _injuryFreeDaysService.GetCurrentInjuryFreeDaysAsync(cancellationToken);

        return new EmployeeSafetyKpiDto(myNearMisses, drafts, submitted, approved, score, injuryFreeDays);
    }

    public async Task<ManagerSafetyKpiDto> GetManagerKpisAsync(CancellationToken cancellationToken = default)
    {
        var today = _dateTimeService.UtcNow.Date;
        var monthStart = new DateTime(_dateTimeService.UtcNow.Year, _dateTimeService.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nearMisses = (await _unitOfWork.NearMisses.GetAllAsync(cancellationToken)).ToList();
        var monthNearMisses = nearMisses.Where(n => n.CreatedDate >= monthStart).ToList();
        var stopReports = (await _unitOfWork.StopUnsafeWorkReports.GetAllAsync(cancellationToken)).ToList();
        var actions = (await _unitOfWork.CorrectiveActions.GetAllAsync(cancellationToken)).ToList();
        var jsas = (await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken)).ToList();
        var injuries = (await _unitOfWork.Injuries.GetAllAsync(cancellationToken)).ToList();
        var users = (await _unitOfWork.Users.GetAllAsync(cancellationToken)).ToList();

        var openInvestigations = nearMisses.Count(n =>
            n.Status is NearMissStatus.Submitted or NearMissStatus.UnderInvestigation);
        var openActions = actions.Count(a =>
            a.Status is CorrectiveActionStatus.Open or CorrectiveActionStatus.InProgress or CorrectiveActionStatus.Overdue);
        var pendingJsas = jsas.Count(j => j.Status == JsaStatus.Draft);
        var submittedToday = jsas.Count(j =>
            j.Status is JsaStatus.Submitted or JsaStatus.Approved or JsaStatus.InReview
            && j.ModifiedDate?.Date == today);
        var injuryFreeDays = await _injuryFreeDaysService.GetCurrentInjuryFreeDaysAsync(cancellationToken);

        var trend = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var start = monthStart.AddMonths(-5 + i);
                var end = start.AddMonths(1);
                var count = nearMisses.Count(n => n.CreatedDate >= start && n.CreatedDate < end);
                return new ChartPointDto(start.ToString("MMM yyyy"), count);
            })
            .ToList();

        return new ManagerSafetyKpiDto(
            monthNearMisses.Count,
            openInvestigations,
            stopReports.Count,
            openActions,
            pendingJsas,
            injuryFreeDays,
            openInvestigations,
            submittedToday,
            users.Count,
            injuries.Count(i => i.InjuryType == InjuryType.LostTimeInjury),
            injuries.Count(i => i.InjuryType == InjuryType.MedicalTreatmentInjury),
            injuries.Count(i => i.InjuryType == InjuryType.FirstAidInjury),
            trend,
            GroupCount(monthNearMisses, n => n.Department),
            GroupCount(monthNearMisses, n => n.Category.ToString()),
            GroupCount(monthNearMisses, n => n.Status.ToString()),
            GroupCount(monthNearMisses, n => n.Department).OrderByDescending(x => x.Count).Take(5).ToList());
    }

    private static IReadOnlyList<NamedCountDto> GroupCount<T>(
        IEnumerable<T> items,
        Func<T, string> selector) =>
        items.GroupBy(selector).Select(g => new NamedCountDto(g.Key, g.Count())).OrderByDescending(x => x.Count).ToList();
}

public class SafetyEscalationService : ISafetyEscalationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISafetyNotificationDispatcher _notifications;
    private readonly IDateTimeService _dateTimeService;

    public SafetyEscalationService(
        IUnitOfWork unitOfWork,
        ISafetyNotificationDispatcher notifications,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _notifications = notifications;
        _dateTimeService = dateTimeService;
    }

    public async Task ProcessOverdueCorrectiveActionsAsync(CancellationToken cancellationToken = default)
    {
        var today = _dateTimeService.UtcNow.Date;
        var actions = await _unitOfWork.CorrectiveActions.GetAllAsync(cancellationToken);
        foreach (var action in actions.Where(a =>
                     a.Status is CorrectiveActionStatus.Open or CorrectiveActionStatus.InProgress
                     && a.DueDate.Date < today))
        {
            action.Status = CorrectiveActionStatus.Overdue;
            _unitOfWork.CorrectiveActions.Update(action);

            var title = "Corrective Action Overdue";
            var message = $"Corrective action is overdue (due {action.DueDate:d}).";

            await _notifications.NotifyUsersAsync(
                new[] { action.AssignedToUserId },
                title,
                message,
                NotificationPriority.High,
                WorkflowNotificationType.CorrectiveActionOverdue,
                relatedEntityType: RelatedEntityTypes.CorrectiveAction,
                relatedEntityId: action.Id,
                cancellationToken);

            await _notifications.NotifyRolesAsync(
                new[] { AppRoles.Supervisor, AppRoles.SafetyManager, AppRoles.HseManager },
                title,
                message,
                NotificationPriority.High,
                WorkflowNotificationType.CorrectiveActionOverdue,
                relatedEntityType: RelatedEntityTypes.CorrectiveAction,
                relatedEntityId: action.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
