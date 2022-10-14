# Logging
## Fluent API

We generate logs using a fluent interface providing a way to log in one sentence all the information we need.</br>


# API
The NuGet package implementing this fluent Api is Arc4u.Standard.Diagnostics.</br>

## Initializing the logger
The fluent Api doesn't replace the .Net Core implementation based on ILogger\<T> but simply enhances the .Net Core implementation.

Basically Serilog is used with configuration using the appSettings file. See [Serilog implementation](./../Serilog.md).

```csharp
            var logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(Configuration)
                            .CreateLogger();

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: logger, dispose: true));
```

### ILogger

An instance of the logger is resolved or preferrably injected in a constructor.

example:
```csharp
   var logger = container.Resolve<ILogger<SerilogTests>>();
```

### LogTypes
Currently there are 3 types/contexts/categories of logs, these being:
- Technical
    - Used to log information about the database, unexpected errors, connections,...
    - Usually info that you don't want the user to see.
- Business
    - Used to log information about the business process and/or errors.
    - Usually info that you want to show to the user.
- Monitoring
    - Used to log connected users, memory consumptions, CPU usage, metrics,...
    - Usually info that you want to expose to your dashboard.


### Demonstration (ILogger\<T>)

The following method logs a Debug message with 2 properties.

```csharp
var logger = container.Resolve<ILogger<T>>();

logger.Technical().Debug("Text").Add("Count", 123).Add("Subject","Subject").Log();

```

If is also possible to add a property if a condition is encountered.

```csharp
var count = 1;

logger.Technical().Debug("Text").AddIf(count > 0, "Count", 123).Log();

```


### Demonstration (ILogger)
When working with ILogger, we still have to provide the *From*, but the syntax has minor changes.

1. We inject our logger into the constructor.</br>
   `public Class(ILogger logger,...)`
2. We provide an instance for our logger.</br>
   `{ _logger = logger; ...}`
3. We define our log as usual.</br>
   `_logger.Technical().From(typeof(TypeToLog)).Warning("Text").Log();`

### Logging Context
Using contexts, you can specify certain things:
- values that always apply within that scope
- filters within that scope
- a scope within the created scope
- ...

This can save you a lot of time and effort, since by manipulation the context itself, you can set certain 'rules'.</br>

e.g.:</br>
If you want to add a custom string to all logs, you could use the AddString("key", "value") on the scope and use that scope.</br>
This way, all logs in that scope get that string and you no longer have to define it for each log seperately.

#### Filters
You can filter which values you want to log.</br>
This can be done by adding a parameter when you create a new context.</br>
When doing so, you can choose out of the following:
- **All**: This is the default value and will result in taking all values set by the previous scope.
- **None**: This will result in a new context with no values in it to be logged yet.
- **Filter**: You can create a custom filter to select which values you want to propagate from the previous scope.

#### Examples
***Logging Examples***</br>
- Creating a context in which all logs will have a PropagatedValue with value 100.</br>
  In the result we will see that the propagated value will be added.

````csharp
    using (new LoggerContext())
    {
        LoggerContext.Current.AddLong("PropagatedValue", 100);   
        Logger.Technical.From<Startup>().Debug("Web Process is started.").Log();    
    }   
````

- Creating a context in which only logs whose key is IsEdible will get a value IsFood in their log.</br>
  The one with key *IsEdible* will now fully be propagated and augmented with IsEdible.</br>
  The one with key *IsObject* won't.

````
    using (new LoggerContext())
    {
        LoggerContext.Current.AddBoolean("IsObject", true);
        LoggerContext.Current.AddBoolean("IsEdible", true);
        using (new LoggerContext((kv) => kv.Key.Equals("IsEdible")))
        {
            LoggerContext.Current.AddBoolean("IsFood", true);
        }
        Logger.Technical.From<Startup>().Debug("Web Process is started.").Log();
    }
````

- Creating a context in which all logs have a Propagated value of 100 and a IsBoolean with value true.

````csharp
    using (new LoggerContext())
    {
        LoggerContext.Current.AddLong("PropagatedValue", 100); 
        using (new LoggerContext())
        {
            LoggerContext.Current.AddLong("IsBoolean", true);        
        }
        Logger.Technical.From<Startup>().Debug("Web Process is started.").Log(); 
    }
````

***Filter Examples***</br>
Here we list some examples on how to implement the filters.</br>
For definitions on what everything means, check [above](#filters).

**All**
````csharp
    using (new LoggerContext(PropertyFilter.All))
        {
            ...
````

**None**
````csharp
    using (new LoggerContext(PropertyFilter.None))
        {
            ...
````

**Filter**
````csharp
    using (new LoggerContext((kv) => kv.Key.Equals("KeyValue")))
        {
            ...
````
