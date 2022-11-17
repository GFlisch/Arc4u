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

    /// <inheritdoc />
    public async Task RunWithinLockAsync(string label, TimeSpan ttl, Func<Task> toBeRun, CancellationToken cancellationToken)
    {
        var lockEntity = await  _lockingDataLayer.TryCreateLockAsync(label, ttl, cancellationToken);
        var refreshRate = _configuration.RefreshRate;
        
        if (lockEntity is not null)
        {
            _logger.LogDebug($"Got a lock for >{label}<. Running >{toBeRun.Method.Name}<");
            using (lockEntity)
            {
                Timer? timer = null;
                try
                {
                    var task = toBeRun();
                    timer = new Timer(state =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            lockEntity.KeepAlive();
                        }
                    }, null, refreshRate, refreshRate);
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
            _logger.LogDebug( $"Could not get a lock for label {label}");
        }
    }

    /// <inheritdoc />
    public async Task<Lock> CreateLock(string label, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var ret = await TryCreateLock(label, ttl, cancellationToken);
        if (ret != null)
        {
            return ret;
        }

        throw new Exception($"Could not obtain a lock for label {label}");
    }
    
    public async Task<Lock?> TryCreateLock(string label,TimeSpan ttl, CancellationToken cancellationToken)
    {
         Timer timer = null;
        var lockEntity = await  _lockingDataLayer.TryCreateLockAsync(label, ttl, CleanUpCallBack, cancellationToken);
        var refreshRate = _configuration.RefreshRate;
        
        if (lockEntity is not null)
        {
            timer = new Timer(state =>
                               {
                                   if (!cancellationToken.IsCancellationRequested)
                                   {
                                       lockEntity.KeepAlive();
                                   }
                               }, null, refreshRate, refreshRate);
        }
        else
        {
            _logger.LogDebug( $"Could not get a lock for label {label}");
        }

        return lockEntity;
        
        async Task CleanUpCallBack()
        {
            if (timer != null)
            {
                await timer.DisposeAsync();
            }
        }
    }
}