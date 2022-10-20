using Microsoft.Extensions.Logging;

namespace Arc4u.Locking.Abstraction;

public class LockingService : ILockingService
{
    private readonly ILockingDataLayer _lockingDataLayer;
    private readonly LockingConfiguration _configuration;
    private readonly ILogger<LockingService> _logger;

    public LockingService(ILockingDataLayer lockingDataLayer, LockingConfiguration configuration, ILogger<LockingService> logger)
    {
        _lockingDataLayer = lockingDataLayer;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task RunWithinLockAsync(string label, TimeSpan maxAge, Func<Task> toBeRun, CancellationToken cancellationToken)
    {
        var lockEntity = await  _lockingDataLayer.TryCreateLockAsync(label, maxAge);
        var ttl = _configuration.RefreshRate;
        
        if (lockEntity is not null)
        {
            _logger.LogDebug($"Got a lock for >{label}<. Running >{toBeRun.Method.Name}<");
            using (lockEntity)
            {
                Timer? timer = null;
                try
                {
                    var task = toBeRun();
                    timer = new Timer(state => lockEntity.KeepAlive(), null, ttl, ttl);
                    await task;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while calling {toBeRun.Method.Name} with label {label}");
                    throw;
                }
                finally
                {
                    if (timer != null)
                    {
                        await timer.DisposeAsync();
                    }
                }
            }
        }
        else
        {
            _logger.LogDebug( $"Got not get a lock for label {label}");
        }
    }
}