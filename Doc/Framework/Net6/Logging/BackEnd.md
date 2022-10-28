# Back End

## CPU & Memory Usage
After initialization, you have to add the following lines of code to add the monitoring:</br>

**.Net**
````csharp 
private static SystemRessources ResourceMonitor { get; set; }
    
public void Initialize()
{
    Logger.Initialize(new LogWriter());
    
    ResourceMonitor = new SystemRessources();
    ResourceMonitor.StartAsync(CancellationToken.None).Wait();
````

**.AspNetCore**
````csharp       
public void ConfigureServices(IServiceCollection services)
{
    services.UseSystemMonitoring();
    ...
}
````

We create a new instance of SystemResources.</br>
This will send a new message with info on the CPU every 10 seconds by default.</br>

## Performance (Metric)

The OAuth service aspect attribute has a monitoring info: *time to complete method call*.</br>
This attribute provides you with 2 properties: 
- **elapsed time**: The time it took to write the log in milliseconds.
- **memory usage** The total memory that was used by the garbage collector to perform the process.

There also is a property named *SetExtraLogging* with a signature *(Action<Type, TimeSpan>)*.</br>
This property can be used to define extra info to add to each call.</br>
In this property, Timespan is the elapsed time and Type is the type of the method you wish to log.

The same metric is available for gRPC calls.


