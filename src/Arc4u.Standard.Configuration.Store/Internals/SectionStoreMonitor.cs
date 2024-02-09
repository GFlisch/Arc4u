using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arc4u.Configuration.Store.Internals;

sealed class SectionStoreMonitor : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SectionStoreMonitor> _logger;
    private readonly TimeSpan _pollingInterval;

    public SectionStoreMonitor(TimeSpan pollingInterval, IServiceScopeFactory serviceScopeFactory, ILogger<SectionStoreMonitor> logger)
    {
        _pollingInterval = pollingInterval;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SectionStoreMonitor)} Service starting");
        return base.StartAsync(cancellationToken);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(SectionStoreMonitor)} Service started");
        try
        {
            do
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var sectionStoreService = scope.ServiceProvider.GetRequiredService<ISectionStoreService>();
                    sectionStoreService.ReloadIfNeeded();
                }
                catch (Exception e) when (e is not OperationCanceledException || !stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(e, $"Error executing SectionStore service");
                }
                await Task.Delay(_pollingInterval, stoppingToken).ConfigureAwait(false);
            }
            while (!stoppingToken.IsCancellationRequested);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // do nothing, the service is stopping
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Periodic Task Exception");
        }
        _logger.LogInformation($"{nameof(SectionStoreMonitor)} Service stopped");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SectionStoreMonitor)} Service stopping");
        return base.StopAsync(cancellationToken);
    }
}
