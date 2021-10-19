# Arc4u

Arc4u means: architecture for you.
The target of this framework is to provide a framework where most of the common features needed by an application is covered.

The new version 3.1.2 is a migration from the .Net one but completely revisited to use the latest feature of .Net Core 3.1.

The features are:
- The caching.
- The Logging with Serilog.
- The serialization used by the caching.
- OAuth2 and OpenId connect (AzureAD and ADFS)
- The Dependency injection.
- NServiceBus.
- KubeMQ
- gRPC
- Entity framework core.
- MongoDB.

The framework is designed to be flexible enough so the application is using an "API" and the implementation can be changed.
For example, the Dependency injection encapsulates the functionality of the container and you can change the container if you want.
Two implementations exist:
- Arc4u.Dependency.Composition (used eagerly by .Net, Mef2).
- Arc4u.Dependency.ComponentModel (used by .Net core 3.1).

Composition is here for backward compatibility with old .Net 4.x applications and I don't encourage you to use this!

During the initialization, one of the container technology is used but when you code your business logic, you are agnostic of the container technology used.

The same philosophy is used for the logging, the serialization, caching and OAuth2 (where differents token provider can be used).

## The framework.

- Arc4u.Standard                                                       
- Arc4u.Standard.AspNetCore.gRpc                                       
- Arc4u.Standard.Caching                                               
- Arc4u.Standard.Caching.Memory                                        
- Arc4u.Standard.Caching.Reddis                                        
- Arc4u.Standard.Caching.Sql                                           
- Arc4u.Standard.Configuration                                         
- Arc4u.Standard.Core                                                  
- Arc4u.Standard.Core.Test                                             
- Arc4u.Standard.Data                                                  
- Arc4u.Standard.Dependency                                            
- Arc4u.Standard.Dependency.ComponentModel                             
- Arc4u.Standard.Dependency.Composition                                
- Arc4u.Standard.Dependency.DryIoc                                     
- Arc4u.Standard.Diagnostics                                           
- Arc4u.Standard.Diagnostics.Serilog                                   
- Arc4u.Standard.Diagnostics.Serilog.Sinks.RealmDb                     
- Arc4u.Standard.Diagnostics.TraceListeners                            
- Arc4u.Standard.EF                                                    
- Arc4u.Standard.EfCore                                                
- Arc4u.Standard.gRPC                                                                                   
- Arc4u.Standard.MongoDB                                               
- Arc4u.Standard.NServiceBus                                           
- Arc4u.Standard.NServiceBus.Core                                      
- Arc4u.Standard.NServiceBus.RabbitMQ                                  
- Arc4u.Standard.OAuth2                                                
- Arc4u.standard.OAuth2.Adal                                           
- Arc4u.Standard.OAuth2.AspNetCore                                     
- Arc4u.Standard.OAuth2.AspNetCore.Api                                 
- Arc4u.Standard.OAuth2.Blazor                                         
- Arc4u.Standard.OAuth2.Client                                         
- Arc4u.Standard.Serializer                                            
- Arc4u.Standard.Serializer.Protobuf                                   
- Arc4u.Standard.Threading                                             
- Arc4u.Universal                                                      
- Arc4u.Windows.Mvvm                                                   
- Arc4u.Xamarin.Android                                                
- Arc4u.Xamarin.Forms.Mvvm                                             
- Arc4u.Xamarin.IOS                                                                                                             
- Prism.ComponentModel.Wpf                                             
- Prism.Mef.Wpf      

The way the framework is built is always based with 2 aspects in mind:
- abstraction
- and injection.

Abstraction allows a developer to work with a concept with no affinity with a particular technology. 
Injection means that the framework resolve implementation based on interfaces so you can change the behaviour as much as possible.

## The Guidance.

Tthe guidance is a visual studio extension that help you to build a micro-services application in an easy way based on the Ar4cu framework!.

### Backend.

The backend is built around the micro-services architecture and to illustrate what the guidance generates. I will take a simple applications with 2 services: Stock and Order.

```csharp

DemoAks
├───.nuget
├───.vs
├───BE
│   ├───Order
│   │   ├───DemoAks.Order.Business
│   │   ├───DemoAks.Order.Domain
│   │   ├───DemoAks.Order.Facade
│   │   ├───DemoAks.Order.IBusiness
│   │   ├───DemoAks.Order.IISHost
│   │   ├───Sdks
│   │   │   └───DemoAks.Order.Facade.Sdk
│   │   └───Tests
│   │       └───DemoAks.Order.IntegrationTest
│   ├───Stock
│   │   ├───DemoAks.Stock.Business
│   │   ├───DemoAks.Stock.Domain
│   │   ├───DemoAks.Stock.Facade
│   │   ├───DemoAks.Stock.IBusiness
│   │   ├───DemoAks.Stock.IISHost
│   │   ├───Sdks
│   │   │   └───DemoAks.Stock.Facade.Sdk
│   │   └───Tests
│   │       └───DemoAks.Stock.IntegrationTest
│   └───Yarp
│       ├───DemoAks.Yarp.Business
│       ├───DemoAks.Yarp.Domain
│       ├───DemoAks.Yarp.Facade
│       ├───DemoAks.Yarp.IBusiness
│       ├───DemoAks.Yarp.IISHost
│       ├───Sdks
│       │   └───DemoAks.Yarp.Facade.Sdk
│       └───Tests
│           └───DemoAks.Yarp.IntegrationTest
└───DemoAks.Shared.Domain

``` 

We can see that we have 3 services in fact. The first one (Yarp) is a reverse proxy one based on the Microsoft Reverse proxy framework.
This service does more than just a reverse proxy one like nginx.
It does:
- reverse proxy.
- Handle authentication (OAuth2/OpenID Connect).
- Api Gateway for Rest Api based on NSwag.
- Study is ongoing to see if doing the same for gRPC (as Swagger) is possible.

You can find more explanation ![here](../Guidance/Yarp.md).

The role of the yarp is mainly to route the traffic and perform the authentication.

The 2 other services are the real ones. See the ![guidance section](../Guidance/Home.md) for more details.





