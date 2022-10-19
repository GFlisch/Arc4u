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

    public async Task RunWithinLock(string label, TimeSpan maxAge, Func<Task> toBeRun)
    {
        var lockEntity = await  _lockingDataLayer.TryCreateLock(label, maxAge);
        if (lockEntity is not null)
        {
            _logger.LogDebug($"Got a lock for >{label}<. Running >{toBeRun.Method.Name}<");
            using (lockEntity)
            {
                var task = toBeRun();

                var ttl = _configuration.RefreshRate;
                Timer timer = new Timer(state => lockEntity.KeepAlive(),null, ttl, ttl);
                await task;
                await timer.DisposeAsync();
            }
        }
        else
        {
            _logger.LogDebug($"Got not get a lock");
        }
    }
}