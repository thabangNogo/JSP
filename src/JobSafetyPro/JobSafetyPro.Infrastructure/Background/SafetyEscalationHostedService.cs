using JobSafetyPro.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobSafetyPro.Infrastructure.Background;

public class SafetyEscalationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SafetyEscalationHostedService> _logger;

    public SafetyEscalationHostedService(
        IServiceProvider serviceProvider,
        ILogger<SafetyEscalationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var escalation = scope.ServiceProvider.GetRequiredService<ISafetyEscalationService>();
                await escalation.ProcessOverdueCorrectiveActionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Safety escalation job failed.");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}
