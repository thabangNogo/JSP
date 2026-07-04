using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Enums;
using JobSafetyPro.Domain.Interfaces;
namespace JobSafetyPro.Application.Services;

public interface ISafetyNotificationDispatcher
{
    Task NotifyUsersAsync(
        IEnumerable<Guid> userIds,
        string title,
        string message,
        NotificationPriority priority,
        WorkflowNotificationType notificationType = WorkflowNotificationType.General,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default);

    Task NotifyRolesAsync(
        IEnumerable<string> roleNames,
        string title,
        string message,
        NotificationPriority priority,
        WorkflowNotificationType notificationType = WorkflowNotificationType.General,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default);
}

public class SafetyNotificationDispatcher : ISafetyNotificationDispatcher
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _hubService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;

    public SafetyNotificationDispatcher(
        IUnitOfWork unitOfWork,
        INotificationHubService hubService,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public async Task NotifyUsersAsync(
        IEnumerable<Guid> userIds,
        string title,
        string message,
        NotificationPriority priority,
        WorkflowNotificationType notificationType = WorkflowNotificationType.General,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var distinct = userIds.Distinct().ToList();
        if (distinct.Count == 0) return;

        var now = _dateTimeService.UtcNow;
        var createdBy = _currentUserService.Email ?? _currentUserService.UserId?.ToString() ?? "system";

        foreach (var userId in distinct)
        {
            var notification = new SafetyNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Priority = priority,
                NotificationType = notificationType,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                CreatedDate = now,
                CreatedBy = createdBy,
            };
            await _unitOfWork.SafetyNotifications.AddAsync(notification, cancellationToken);
            await _hubService.SendToUserAsync(userId, title, message, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task NotifyRolesAsync(
        IEnumerable<string> roleNames,
        string title,
        string message,
        NotificationPriority priority,
        WorkflowNotificationType notificationType = WorkflowNotificationType.General,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetUsersInRolesAsync(
            roleNames,
            _currentUserService.CompanyId,
            cancellationToken);
        await NotifyUsersAsync(
            users.Select(u => u.Id),
            title,
            message,
            priority,
            notificationType,
            relatedEntityType,
            relatedEntityId,
            cancellationToken);
    }
}
