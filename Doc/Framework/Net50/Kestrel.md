# Kestrel and configuration files.

We have a lot f possible way to deploy a service and different kestrel configurations to define.

I will use this configuration as a support:

                    |---------------|         |---------------|
                    |               |         |               |
                    |               | http/2  |               |
                    |               | <---->  |    Service1   |
                    |               |         |               |
        http/1-/2   |               |         |               |
      <---------->  |               |         |---------------|
        TLS1.2      |      RP       |      
                    |               | 
                    |               |         |---------------|   
                    |               |         |               |
                    |               | http/2  |               |
                    |               | <---->  |   Service2    |
                    |               |         |               |
                    |---------------|         |               |
                                              |---------------|


## Standalone on windows server.

When you install a service on a windows server, you have basically 2 choices:
- on IIS.
- as a Windows NT Service.

Both are valids but IIS is limited concerning gRPC. If you have a service running gRPC, you have to install it as a Windows service.

All the communication between the services will be secured via TLS 1.2 (https).

The reverse proxy (I use [Yarp.ReverseProxy](https://github.com/microsoft/reverse-proxy)) is the service talking to all the parties. 
It means that UI applications will also communicate with it and we have to be able to serve:
- Http/1.1
- Http/2
- Http/3 in the future.

As the browser for the moment are unable to communicate via Http/2, Http/1.1 is mandatory (the same for tools like Postman).

The other consumers (Wpf, Uwp, Xamarin Forms) will communicate via Http/2 when in gRPC.

One important concept to understand when you build the service is the capability to have a service that you can deploy on different platforms:
- Windows (IIS and Nt Service).
- Linux on Kubernetes.

In AspNetCore the guidance tool will create a service able to be deployed on those 3 "medias".

To be able to deploy as Windows NT service the program.cs class is configured like this.

```csharp

        public static IHostBuilder CreateHostBuilder(string[] args)
        {

            return Host.CreateDefaultBuilder(args)
            ...
                .UseWindowsService()
            ...
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }

```

When you build this service on a Windows build machine you will have an executable that you can deploy as a service or use the dlls to deploy on IIS.

### IIS kestrel configuration.

Again, in this case your service will only contain Rest APIs, asynchronous comunications and no gRPC.

Nothing has to be configured when using IIS but the ususal configuration of the application pool and SSL part.

Just deploy your application and don't forget to install the Hosting bundle installer, more info can be find [here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-5.0).

### Nt Service.

As the service NT will be executed on the Kestrel framework we have to configure Kestrel
All the communication in those cases are encrypted! We have to inform the configuration in the appSettings.json file the certificate to use.

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:9900",
        "Protocols": "Http1AndHttp2",
        "Scheme": "https",
        "Certificate": {
          "Subject": "*.arc4u.net",
          "Store": "My",
          "Location": "LocalMachine",
          "AllowInvalid": true
          }
        }
	  }
    }
}
```


So Yarp or services will have the same configuration but the protocol where the services behind the reverse proxy are communicating in http/2 => so protocols will be set to Http2.

### Linux and Kubernetes.

When we deploy on Kubernetes we don't embed the config file (appsettings.json) in the container but glue this via ConfigMap.

The hosting is also done via Kestrel and so we have to configure this.

I will do the distinction between the Yarp (reverse proxy) and the services behind it.

Remember (from the schema on top of this page) the global architecture, in Kubernetes the services behind the reverse proxy are communicating in Http/2 but in
plain text  because they are not exposed directly (ClusterIP).

So we can assume that there is no need to have a certificate even if this is completely possible.



#### Yarp.

We will have a look first at the reverse proxy one.

```json
{
  "Kestrel": {
    "EndPoints": {
      "Default": {
        "Url": "https://*:5001",
        "Protocols": "Http1AndHttp2",
        "Certificate": {
          "Path": "certs/tls.crt",
          "KeyPath": "certs/tls.key"
        }
      },
      "internal": {
        "Url": "http://*:5000",
        "Protocols": "Http2"
      },
      "Probe": {
        "Url": "http://*:8080",
        "Protocols": "Http1"
      }
    }
  }
}
```

First of all, the Kestrel is configured to be serving requests via TLS 1.2 and the default port defined by AspNetCore for this is 5001.
The protocols defined is Http1AndHttp2 and certificate are added via Secret and included to the image via volume in Kubernets.

To avoid https communication between internal services in Kubernetes, an second internal endpoint is defined with http/2 as protocol.

A third endpoint defined is a "non secured" one for the readiness and liveness feature of Kubernetes. This will serve the HealthCheck part of the AspNetCore framework
and will not be exposed outside of the pod. This is the puropose of the Probe endpoint defined in plain text and in Http/1


#### Services.

At the level of the services behind the reverse proxy, no TLS 1.2 is used => the Kestrel config is then easier.

```json
{
  "Kestrel": {
    "EndPoints": {
      "Default": {
        "Url": "http://*:5000",
        "Protocols": "Http2"
      },
      "Probe": {
        "Url": "http://*:8080",
        "Protocols": "Http1"
      }
    }
  },
}

Here the default http AspNetCore 5000 is used (plain text).
The communication between the reverse proxy and the services are done in Http/2.

We create also an endpoint not exposed for the readiness and liveness feature of Kubernetes.


```
