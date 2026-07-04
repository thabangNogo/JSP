using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Workflow;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public interface IManagerDashboardService
{
    Task<ManagerPendingActionsDto> GetPendingActionsAsync(CancellationToken cancellationToken = default);
}

public class ManagerDashboardService : IManagerDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public ManagerDashboardService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task<ManagerPendingActionsDto> GetPendingActionsAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();

        var jsas = await _unitOfWork.JobSafetyAssessments.GetAllAsync(cancellationToken);
        var nearMisses = await _unitOfWork.NearMisses.GetAllAsync(cancellationToken);
        var stopWork = await _unitOfWork.StopUnsafeWorkReports.GetAllAsync(cancellationToken);
        var injuries = await _unitOfWork.Injuries.GetAllAsync(cancellationToken);
        var correctiveActions = await _unitOfWork.CorrectiveActions.GetAllAsync(cancellationToken);
        var today = _dateTimeService.UtcNow.Date;

        var pendingJsas = jsas.Where(j => j.Status == JsaStatus.Submitted).ToList();
        var nearMissPending = nearMisses.Where(n =>
            n.Status is NearMissStatus.Submitted or NearMissStatus.UnderInvestigation).ToList();
        var overdue = correctiveActions.Where(c =>
            c.DueDate.Date < today &&
            c.Status is not CorrectiveActionStatus.Completed
                and not CorrectiveActionStatus.Verified
                and not CorrectiveActionStatus.Closed).ToList();
        var openInjuries = injuries.Where(i => i.Status != InjuryStatus.Closed).ToList();
        var openStopWork = stopWork.Where(s => s.Status != StopUnsafeWorkStatus.Closed).ToList();
        var draftJsas = jsas.Where(j => j.Status == JsaStatus.Draft).ToList();

        var items = new List<PendingActionItemDto>();
        items.AddRange(pendingJsas.Select(j => new PendingActionItemDto(
            "JSA", j.Id, j.Title, j.Status.ToString(), j.CreatedDate)));
        items.AddRange(nearMissPending.Select(n => new PendingActionItemDto(
            "NearMiss", n.Id, n.Description.Length > 60 ? n.Description[..60] + "…" : n.Description,
            n.Status.ToString(), n.CreatedDate)));
        items.AddRange(overdue.Select(c => new PendingActionItemDto(
            "CorrectiveAction", c.Id, c.Description.Length > 60 ? c.Description[..60] + "…" : c.Description,
            "Overdue", c.CreatedDate)));
        items.AddRange(openInjuries.Select(i => new PendingActionItemDto(
            "Injury", i.Id, i.EmployeeName, i.Status.ToString(), i.CreatedDate)));
        items.AddRange(openStopWork.Select(s => new PendingActionItemDto(
            "StopUnsafeWork", s.Id, s.Description.Length > 60 ? s.Description[..60] + "…" : s.Description,
            s.Status.ToString(), s.CreatedDate)));

        return new ManagerPendingActionsDto(
            pendingJsas.Count,
            nearMissPending.Count,
            overdue.Count,
            openInjuries.Count,
            openStopWork.Count,
            draftJsas.Count,
            items.OrderByDescending(i => i.CreatedDate).Take(50).ToList());
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
}

public interface IAuditQueryService
{
    Task<IReadOnlyList<AuditLogDto>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);
}

public class AuditQueryService : IAuditQueryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AuditQueryService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();
        var logs = await _unitOfWork.AuditLogs.FindAsync(
            a => a.EntityType == entityType && a.EntityId == entityId,
            cancellationToken);
        return logs.OrderByDescending(a => a.Timestamp)
            .Select(Map)
            .ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        EnsureSafetyLead();
        var logs = await _unitOfWork.AuditLogs.GetAllAsync(cancellationToken);
        return logs.OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(Map)
            .ToList();
    }

    private static AuditLogDto Map(Domain.Entities.Shared.AuditLog a) =>
        new(a.Id, a.Action, a.EntityType, a.EntityId, a.UserId, a.OldValues, a.NewValues, a.Timestamp);

    private void EnsureSafetyLead()
    {
        if (!_currentUserService.Roles.Any(r =>
                r is AppRoles.Administrator or AppRoles.HseManager or AppRoles.SafetyManager
                    or AppRoles.SafetyOfficer))
        {
            throw new UnauthorizedAppException("Safety lead access required.");
        }
    }
}
