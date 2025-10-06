using PandesalExpress.Infrastructure.Services;

namespace PandesalExpress.Host.Services;

public class JwksRefreshService : BackgroundService
{
    private readonly ILogger<JwksRefreshService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5);
    private readonly IServiceProvider _serviceProvider;

    public JwksRefreshService(
        IServiceProvider serviceProvider,
        ILogger<JwksRefreshService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JWKS Refresh Background Service started");

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                FacePublicKeyService keyService = scope.ServiceProvider.GetRequiredService<FacePublicKeyService>();

                _logger.LogDebug("Refreshing JWKS keys in background");
                await keyService.RefreshKeysAsync();

                // Get rotation info to adjust refresh interval
                FacePublicKeyService.RotationInfo? rotationInfo = await keyService.GetRotationInfoAsync();
                TimeSpan nextRefresh = _refreshInterval;

                if (rotationInfo?.RotationIntervalMinutes > 0)
                    nextRefresh = TimeSpan.FromMinutes(rotationInfo.RotationIntervalMinutes / 3);

                _logger.LogDebug("Next JWKS refresh in {NextRefresh}", nextRefresh);
                await Task.Delay(nextRefresh, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing JWKS keys");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

        _logger.LogInformation("JWKS Refresh Background Service stopped");
    }
}
