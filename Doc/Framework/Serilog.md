# Serilog

The Arc4u framework is using the Microsoft Logging framework of AspNetCore.

The configuration in the startup.cs is based on a section defined in the appSettings structure. 
This help to build a strategy different regarding the environment where you deploy your services.

This is the references added in the IISHost project.

```xml
    <PackageReference Include="Serilog" Version="2.10.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="4.1.0" />
    <PackageReference Include="Serilog.Expressions" Version="2.0.0" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="3.1.0" />
    <PackageReference Include="Serilog.Enrichers.Environment" Version="2.1.3" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="3.0.1" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Async" Version="1.4.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="4.1.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="3.1.1" />
```

I have added the capability to store logs on files but this is only for documentation. I strongly discourage you to use this possibility but log to Seq.

The references must be more:

```xml
    <PackageReference Include="Serilog" Version="2.10.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="4.1.0" />
    <PackageReference Include="Serilog.Expressions" Version="2.0.0" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="3.1.0" />
    <PackageReference Include="Serilog.Enrichers.Environment" Version="2.1.3" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="3.0.1" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="3.1.1" />
```

Why the usage of the Console sink?

When we develop, the usage of the Console sink is usefull to highlight the latest informatinon coming
but the usage of the Console sink is more interesting in Kubernetes.
By selecting only a filtering starting from the level Warning, it is interesting for the Kubernetes operators to check
if the pod behaves correctly (no errors or important messages).

The operator will typically execute the command:
</br>kubectl logs \{pod name\}

On a concete example:

```txt

kubectl get pods

kubemq-cluster-0                   1/1     Running   0          8d
kubemq-cluster-1                   1/1     Running   0          8d
kubemq-cluster-2                   1/1     Running   0          8d
kubemq-dashboard-74d97759d-nm675   2/2     Running   0          33d
kubemq-operator-66d949996f-b8xrh   1/1     Running   2          17d
myrabbit-rabbitmq-0                1/1     Running   0          18d
order-774df857f4-qk2h5             2/2     Running   1          43h
order-774df857f4-xx2w5             2/2     Running   1          43h
order-rabbit-684c48d55d-82k98      1/1     Running   0          17d
redis-master-0                     1/1     Running   0          16d
seq-dff85b86-s9knh                 1/1     Running   0          16d
sql-545474d94c-v76bc               1/1     Running   0          31d
stock-6d96bcc75-bgkvk              2/2     Running   1          42h
stock-6d96bcc75-t5jfk              2/2     Running   1          42h
yarp-748468c4d5-nbzs4              1/1     Running   0          15d
yarp-748468c4d5-v6mcf              1/1     Running   0          15d


kubectl logs order-774df857f4-xx2w5 --container order

[11:06:54 Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository [Warning] Storing keys in a directory '"/root/.aspnet/DataProtection-Keys"' that may not be persisted outside of the container. Protected data will be unavailable when container is destroyed.
[11:06:54 Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager [Warning] No XML encryptor configured. Key {ec23d6c6-6823-43b9-bf8e-c2a220f34177} may be persisted to storage in unencrypted form.
[11:06:55 Microsoft.AspNetCore.Server.Kestrel [Warning] Overriding address(es) '"http://+:80"'. Binding to endpoints defined in "UseKestrel()" instead.

```

## Creation of the logger.

In the startup.cs class we have to create the logger that will setup the logger based on the Configuration instance.

```csharp

            var logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(Configuration)
                            .CreateLogger();

            services.AddApplicationContext();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: logger, dispose: true));
            services.AddAuthorizationCheckAttribute();


```

## Configurations.

In Arc4u we have extensions method to extend the logging feature of the .Net framework. [See Logging](./Net50/Logger.md).

And there is still the .Net way of logging information used by the libraries.

For the moment the Arc4u framework has it's own filtering and application name configuration put in the 

```json
{
  "Application.Configuration": {
    "ApplicationName": "Demo.Core",
    "Environment": {
      "name": "Stock",
      "loggingName": "Demo.Core",
      "loggingLevel": "Trace",
      "timeZone": "Romance Standard Time"
    }
  }
}
```
Application.Configuration:Environment:loggingName is used to name the file and Application.Configuration:Environment:loggingLevel is used as
a general filtering for the Arc4u logging extension.
As we use Serilog and we configure this now based on the appSettings file, the use of this is not interesting. A future build of the framework will
suppress those 2 elements. The loggingLevel is set to Trace meaning no filering and the filtering will be done at the level of the Serilog framework.

Arc4u is independant of Serilog (only ILogger is used) so another library can be chosen.

### Serilog.

The configuration section is structured like this:
- General (common) setup.
- Dedicated Sink configuration.

#### General setup.

This is the configuration used to inject, filter, etc... logs.

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Expressions",
      "Serilog.Sinks.Console",
      "Serilog.Sinks.Seq",
      "Serilog.Sinks.File",
      "Serilog.Sinks.Async"
    ],
    "MinimumLevel": {
      "Default": "Verbose",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Hangfire": "Warning",
        "KubeMQ.SDK": "Warning",
        "Grpc": "Warning",
        "NServiceBus": "Warning"
      }
    },

    "Enrich": [ "WithMachineName" ],
    "Properties": {
      "Application": "Demo.Core"
    }
  }
}
```

The using section defines the Sinks and feature we are using. In this full example all the sinks will be used but you have only
to add those you need. The Guidance is actually addind those from the nuget store.

The default minimul level is used to define the threshold Serilog is using to take or not the log. The override section is to specify by
namespace name (StartWith) the new rule to apply. A lot of messages are coming from the framework to use and only Warnings abd above are considered.
Up to you to decide what you want to select!

Enrich is a way to inject some properties. I add the machine name so we can know from which server this service is runnig. This feature is
automatic when you use the Arc4u logging but not when using the logging from .Net core. So doing this we have at least this information and we 
are able to know from which instance this message is coming.

The same for the Application, this information is injected so we are able to know from which application the message has been generated.

This configuration doesn't contains any WriteTo section meaning than no output will be receiving logs.

This is the different sections for different logger...

I am using the capacity to customize each logger independendtly. So each level can define its own filtering level, filters, etc... But the MinimumLevel defined is always at least defined by the general settings!

##### Console logger.

This is configuring the Console logger and excluding the Monitor (Category = 4) messages.

Need to define the using of Serilog.Sinks.Console.

```json
    "WriteTo:ConsoleLogger": {
      "Name": "Logger",
      "Args": {
        "configureLogger": {
          "MinimumLevel": "Information",
          "Filter": [
            {
              "Name": "ByExcluding",
              "Args": {
                "expression": "Category = 4"
              }
            }
          ],
          "WriteTo": [
            {
              "Name": "Console",
              "Args": {
                "outputTemplate": "[{Timestamp:HH:mm:ss} {SourceContext} [{Level}] {Message}{NewLine}{Exception}",
                "theme": "Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme::Grayscale, Serilog.Sinks.Console"
              }
            }
          ]
        }
      }
    },
```

##### Seq logger.

This is configuring the Seq logger and excluding the Monitor (Category = 4) messages in the Localhost environment (developer machine).
When deloying on a server do not filter any messages.

It is interesting to sometimes do not exclude monitoring messages to check that you don't have a  memory leak (by checking the memory comsumption).

Need to define the using of Serilog.Sinks.Seq.

```json
    "WriteTo:SeqLogger": {
      "Name": "Logger",
      "Args": {
        "configureLogger": {
          "MinimumLevel": "Debug",
          "Filter": [
            {
              "Name": "ByExcluding",
              "Args": {
                "expression": "Category = 4"
              }
            }
          ],
          "WriteTo": [
            {
              "Name": "Seq",
              "Args": {
                "serverUrl": "http://localhost:5341"
              }
            }
          ]
        }
      }
    },
```

##### File logger.

This is configuring the Console logger and excluding the Monitor (Category = 4) messages.

Need to define the using of Serilog.Sinks.Async and Serilog.Sinks.File.

```json
    "WriteTo:FileLogger": {
      "Name": "Logger",
      "Args": {
        "configureLogger": {
          "MinimumLevel": "Debug",
          "Filter": [
            {
              "Name": "ByExcluding",
              "Args": {
                "expression": "Category = 4"
              }
            }
          ],
          "WriteTo": [
            {
              "Name": "Async",
              "Args": {
                "configure": [
                  {
                    "Name": "File",
                    "Args": {
                      "path": "Log\\log-.txt",
                      "rollingInterval": "Hour",
                      "fileSizeLimitBytes": 26214400,
                      "rollOnFileSizeLimit": true,
                      "buffered": true,
                      "shared": false,
                      "flushToDiskInterval": "00:00:01",
                      "bufferSize": 25000,
                      "blockWhenFull": true
                    }
                  }
                ]
              }
            }
          ]
        }
      }
    },
    "Enrich": [ "WithMachineName" ],
    "Properties": {
      "Application": ""
    }
  }
```







