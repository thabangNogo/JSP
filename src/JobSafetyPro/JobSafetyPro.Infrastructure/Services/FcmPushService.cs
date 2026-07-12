using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace JobSafetyPro.Infrastructure.Services;

/// <summary>
/// Sends push notifications via Firebase Cloud Messaging when configured.
/// Falls back to logging when FCM server key is not set (notification history is still stored via SafetyNotificationDispatcher).
/// </summary>
public class FcmPushService : IFcmPushService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FcmPushService> _logger;

    public FcmPushService(
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<FcmPushService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default)
    {
        var serverKey = _configuration["Firebase:ServerKey"];
        if (string.IsNullOrWhiteSpace(serverKey))
        {
            _logger.LogDebug("FCM ServerKey not configured. Skipping push for user {UserId}: {Title}", userId, title);
            return;
        }

        var devices = (await _unitOfWork.UserDevices.GetAllAsync(cancellationToken))
            .Where(d => d.UserId == userId && !string.IsNullOrWhiteSpace(d.FcmToken))
            .ToList();

        if (devices.Count == 0) return;

        var client = _httpClientFactory.CreateClient("Fcm");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={serverKey}");

        foreach (var device in devices)
        {
            try
            {
                var payload = new
                {
                    to = device.FcmToken,
                    notification = new { title, body = message },
                    data = new { type = "ppe", userId = userId.ToString() },
                };
                await client.PostAsJsonAsync("https://fcm.googleapis.com/fcm/send", payload, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FCM push failed for device {DeviceId}", device.Id);
            }
        }
    }
}
