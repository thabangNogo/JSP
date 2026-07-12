namespace JobSafetyPro.Application.Interfaces;

public interface IFcmPushService
{
    Task SendToUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
}
