# Architecture

We have different concepts in the architecture and before digging into detail, the most important is to understand those key concepts.



            UI                                                            BE

    -----------------                               |---------------|------------|------------|
    |               |          Rest and gRPC        |               |            |            |---> MongoDB
    | UWP           |  <------------------------>   |   Facade      |    B  L    |     Dal    |---> Sql Server
    | Xamarin.Forms |          Http/2               |      +        |    u  a    |            |---> Neo4J    
    |               |                               |    Dtos       |    s  y    |            |
    |---------------|                               |---------------|    i  e    |------------|
    |               |          Rest                 |               |    n  r    |            |---> General I/O activities
    |    VueJS      |  <------------------------>   |   Interface   |    e       |    Service |     Files, Rest and gRPC, Ftp
    |    Blazor     |          Http/1               |      +        |    s       |    Agent   |     etc...
    |               |                               |    Dtos       |    s       |            |
    |---------------|                               |---------------|------------|------------|
                                                                    |            | 
                                                                    |   Domain   | 
                                                                    |   Model    |
                                                                    |            | 
                                                                    |------------|

Everything is decoupled via Dependency injection.

## Backend.

### Facade

The facade is simply a service layer that will perform dedicated requests/responses for UI needs.
The technologies used to communicate is Rest Apis or gRPC and everything is protected by OAuth2/OpenID Connect.

There is no versionning implemented at the level of facade services => when we do a UI change and we need to update a service, all the UI are impacted.
Which is not a problem because the new need is always something the business want on all the UIs they have.

As performance is important at the level of the UI, specifics Dtos is a valid options (reducing the size of object to transfer).

### Interface.

The interface layer is technically the same as a facade: a service layer implementing Rest and gRpc but with the versionning.
It means that when you  expose a new version information you create a new version of the service and maintain (when this is possible) the backward compatibility
with previous versions of the services. 
Those services are called only by other applications and maintaining the backward compatibility allow to gracefully introduce new concepts but also to let
the other performs the migrations to the latest version not at the same time to the new service deployed ==> this is a key concept!

The asynchronous messages (message broker) handlers are implemented also at the interface layer => messages are  also a backend to backend communication.

#### Dtos.

We use Automapper (when extreme performance is not needed) to map the object of the domain model (POCO) to the Dtos.
In case large set of data are to map, manual mapping will be probably the best choice => but do allways an evaluation of this.

### Service Agent.

Service agents are projects that will handle the specific communication to external world.

### DataLayer

This is a specific service agent dedicated to target database access.

## UIs

We have different UI technologies: Modern ones (Uwp, XF, VueJS) and old one (Wpf).

The Xaml world (Uwp, XF and Wpf) are using MVVM pattern with Http/2 communication.

VueJS is using Rest Apis only with Http/1.

# Micro-services.

When we build an application based on micro-services architecture we have the following picture.

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
                    
                                            |-------------------------------------------|
                                            |     Asynchronous communication            |
                                            |-------------------------------------------|

The reverse proxy (Microsoft reverse proxy) does the job to route the traffic with:
- Http/1 to Http/2
- Http/2 to Http/2
- Tls 1.2 to clear (in K8s)
- Tls 1.2 to Tls 1.2  when deployed in a non K8s environment.
- Api Gateway.
- OAuth 2.0 / OpenID Connect
- gRPC and Rest
- HealthCheck portal => AspNetCore.HealthChecks.

