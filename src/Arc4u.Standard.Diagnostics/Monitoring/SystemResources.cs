using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics.Monitoring
{
    /// <summary>
    /// Monitor and log the information about the Process Cpu and Memory usage!
    /// </summary>
    public sealed class SystemResources : IHostedService, IDisposable
    {
        public SystemResources(ILogger logger, uint internalPeriodInSeconds = 10)
        {
            if (null == logger)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (0 == internalPeriodInSeconds)
            {
                throw new ArgumentException("The interval must be bigger than 0!");
            }

            interval = internalPeriodInSeconds;
            _lastTimeStamp = _process.StartTime.ToUniversalTime();
            _logger = logger;

        }

        private readonly uint interval;
        private readonly ILogger _logger;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = new Timer(CollectData, null, TimeSpan.FromSeconds(StartMonitoringDelayInSeconds), TimeSpan.FromSeconds(interval));

            return Task.CompletedTask;
        }

        private Process _process = Process.GetCurrentProcess();
        private DateTime _lastTimeStamp;
        private TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;
        private TimeSpan _lastUserProcessorTime = TimeSpan.Zero;
        private TimeSpan _lastPrivilegedProcessorTime = TimeSpan.Zero;

        private readonly CpuData _cpuData = new CpuData();

        private void CollectData(object state)
        {
            _process = Process.GetCurrentProcess();

            double totalCpuTimeUsed = _process.TotalProcessorTime.TotalMilliseconds - _lastTotalProcessorTime.TotalMilliseconds;
            double privilegedCpuTimeUsed = _process.PrivilegedProcessorTime.TotalMilliseconds - _lastPrivilegedProcessorTime.TotalMilliseconds;
            double userCpuTimeUsed = _process.UserProcessorTime.TotalMilliseconds - _lastUserProcessorTime.TotalMilliseconds;

            _lastTotalProcessorTime = _process.TotalProcessorTime;
            _lastPrivilegedProcessorTime = _process.PrivilegedProcessorTime;
            _lastUserProcessorTime = _process.UserProcessorTime;

            double cpuTimeElapsed = (DateTime.UtcNow - _lastTimeStamp).TotalMilliseconds * Environment.ProcessorCount;
            _lastTimeStamp = DateTime.UtcNow;

            _cpuData.TotalCpuUsed = totalCpuTimeUsed * 100 / cpuTimeElapsed;
            _cpuData.PrivilegedCpuUsed = privilegedCpuTimeUsed * 100 / cpuTimeElapsed;
            _cpuData.UserCpuUsed = userCpuTimeUsed * 100 / cpuTimeElapsed;

            _logger.Monitoring()
                   .From<SystemResources>()
                   .Information("Cpu & Memory")
                   .Add("TotalCpuUsed", _cpuData.TotalCpuUsed)
                   .Add("PrivilegedCpuUsed", _cpuData.PrivilegedCpuUsed)
                   .Add("UserCpuUsed", _cpuData.UserCpuUsed)
                   .Add("WorkingSet", _process.WorkingSet64)
                   .Add("NonPagedSystemMemory", _process.NonpagedSystemMemorySize64)
                   .Add("PagedMemory", _process.PagedMemorySize64)
                   .Add("PagedSystemMemory", _process.PagedSystemMemorySize64)
                   .Add("PrivateMemory", _process.PrivateMemorySize64)
                   .Add("VirtualMemoryMemory", _process.VirtualMemorySize64)
                   .Log();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Scheduler?.Change(Timeout.Infinite, Timeout.Infinite);

            return Task.CompletedTask;

        }

        private Timer Scheduler { get; set; }

        /// <summary>
        /// Define the delay the system will wait before starting the monitoring.
        /// Default is 10.
        /// </summary>
        public uint StartMonitoringDelayInSeconds { get; set; } = 10;

        public void Dispose()
        {
            Scheduler?.Dispose();
        }
    }
}
