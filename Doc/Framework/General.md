# Arc4u

Arc4u means: architecture for you.
</br>The target is to provide a framework where most of the common features needed by an application is covered.

The versionning is following the releases of Microsoft .NET. 

For example, the version 6.0.9.1 is the version built with the version 6.0.9 of the .NET framework. The .1 is there to indicate the patch number of the Arc4u version.

The features are:
- The caching.
- The Logging with Serilog.
- The serialization used by the caching.
- OAuth2 and OpenId connect (AzureAD, ADFS and AvreAD B2c). A version generic based on OpenIdConnect will come to target also Keycloak, Forgerock, etc...
- The Dependency injection.
- NServiceBus.
- KubeMQ
- gRPC
- Entity framework core.
- MongoDB.

The framework is designed to be flexible enough so the application is using an "API" and the implementation can be changed.
For example, the Dependency injection encapsulates the functionality of the container and you can change the container if you want.
Two implementations exist:
- Arc4u.Dependency.Composition (used eagerly by .Net, Mef2). => this is removed and deprecated from 6.0 (from Arc4u point of view)
- Arc4u.Dependency.ComponentModel (used by .Net core).

Composition is here for backward compatibility with old .Net 4.x applications and I don't encourage you to use this!

During the initialization, one of the container technology is used but when you code your business logic, you are agnostic of the container technology used.

The same philosophy is used for the logging, the serialization, caching and OAuth2 (where differents token provider can be used).

All the projects are available on [nuget.org](https://www.nuget.org/packages?q=arc4u.standard).

Sonarque is used to analyze the code.</br>


> Sonarqube result:
>
> [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=GFlisch_Arc4u&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=GFlisch_Arc4u)<br>
> [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=GFlisch_Arc4u&metric=bugs)](https://sonarcloud.io/summary/new_code?id=GFlisch_Arc4u)<br>
> [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=GFlisch_Arc4u&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=GFlisch_Arc4u)<br>
>


## The framework.

The purpose of the framework is not to rebuild what exist but to use what exist and rely on it by improving or filling some gaps.</br>
For example, an application build with Arc4u will use the OpenTelemetry but the need to extend the current implementation is unecessary. This is why there is no Arc4u.Standard.OpenTelemetry.

The way the framework is built is always based with 2 aspects in mind:<br>
- abstraction
- and injection.

Abstraction allows a developer to work with a concept with no affinity with a particular technology. 
Injection means that the framework resolve implementation based on interfaces so you can change the behavior as much as possible.

We can group the features like this.

### Fundation

- Arc4u.Standard.Core
- Arc4u.Standard
- Arc4u.Standard.Configuration
- Arc4u.Standard.Threading
- Arc4u.Standard.Data

[More here](./General/Fundation.md).

### Caching

- Arc4u.Standard.Caching
- Arc4u.Standard.Caching.Dapr
- Arc4u.Standard.Caching.Memory
- Arc4u.Standard.Caching.Redis
- Arc4u.Standard.Caching.Sql

[More here](./General/Caching.md).

### Serialization

- Arc4u.Standard.Serializer
- Arc4u.Standard.Serializer.BSon
- Arc4u.Standard.Serializer.JSon
- Arc4u.Standard.Serializer.Protobuf
- Arc4u.Standard.Serializer.ProtobufV2


### DI

- Arc4u.Standard.Dependency
- Arc4u.Standard.Dependency.ComponentModel

### Diagnostic (Logging).

- Arc4u.Standard.Diagnostics
- Arc4u.Standard.Diagnostics.Serilog
- Arc4u.Standard.Diagnostics.Serilog.Sinks.RealmDb

### Rules

- Arc4u.Standard.FluentValidation

### gRPC communication.

- Arc4u.Standard.gRPC
- Arc4u.Standard.AspNetCore.gRpc

### Messaging platform.

#### KubeMQ messaging platform.

- Arc4u.Standard.KubeMQ
- Arc4u.Standard.KubeMQ.AspNetCore

#### RabbitMQ via NServiceBus

- Arc4u.Standard.NServiceBus
- Arc4u.Standard.NServiceBus.RabbitMQ

### Database

- Arc4u.Standard.EfCore
- Arc4u.Standard.MongoDB
- Arc4u.Standard.Data

### OpenID Connect / OAuth2

- Arc4u.Standard.OAuth2.AspNetCore.Msal
- Arc4u.Standard.OAuth2.Msal
- Arc4u.Standard.OAuth2
- Arc4u.standard.OAuth2.Adal
- Arc4u.Standard.OAuth2.AspNetCore
- Arc4u.Standard.OAuth2.AspNetCore.Adal
- Arc4u.Standard.OAuth2.AspNetCore.Api
- Arc4u.Standard.OAuth2.AspNetCore.Authentication
- Arc4u.Standard.OAuth2.AspNetCore.Blazor
- Arc4u.Standard.OAuth2.Blazor
- Arc4u.Standard.OAuth2.Client

### UIs

- Arc4u.Windows.Mvvm
- Arc4u.Xamarin.Android
- Arc4u.Xamarin.Forms.Mvvm
- Arc4u.Prism.DI.Wpf


## The Guidance.

The guidance is a visual studio extension that help you to build a micro-services application in an easy way based on the Ar4cu framework!

You will find more info on the [Guidance doc repo](https://github.com/gflisch/arc4u.guidance.doc).


