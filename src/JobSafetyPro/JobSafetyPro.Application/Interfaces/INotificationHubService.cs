namespace JobSafetyPro.Application.Interfaces;

public interface INotificationHubService
{
    Task SendToUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
}
