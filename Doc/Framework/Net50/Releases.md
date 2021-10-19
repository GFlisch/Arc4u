# Arc4u.Standard 5.0: Releases

#### 5.0.10.3
30 Septembre 2021

- FluentValidation:
  - Add IsUtcDateTime rule.
  - Add IsDate rule => check the DateTime is in fact a Date.

#### 5.0.10.2
29 Septembre 2021

- Change FluentValidation rules to work on the IPersistEntity interface and no more the PersistEntity type.
  - Impact the IsInsert(), IsUpdate(), IsDelete() and IsNone() rules.
- Migrate IPersistEntity and PersistChange from Arc4u.Standard.Data to Arc4u.Standard package.
  - => the idea is to use only this concept in the Domain model!
  - ChangeTracker in EfCore is now based on IPersistEntity and no more PersistEntity! 

#### 5.0.10.1
28 Septembre 2021

- Make authentication popup window for Arc4u.Standard.Blazor relative to the base path of the application and not from the root of the web site.
- Add Arc4u.Standard.FluentValidation for PersistEntity => IsInsert(), IsUpdate(), IsDelete() and IsNone().

#### 5.0.8.2
19 August 2021

- Fix an issue with ADFS and the access token type returned by the CredentialTokenProvider.

By fixing this issue the JwtHttpHandler will handle correctly the AuthenticationType when set to "Inject". This settings injects the bearer token
in any case.

#### 5.0.8.1
11 July 2021

- Update to .Net 5.0.8
- Refactoring of JwtHttpHandler.
- Fix issue with gRPC and ClientErrorInterceptor.
- EfCore: ChangeTracker is now based on PersistEntity.
- Blazor:
    - Add capability to change the Culture.
    - Use Policy based on the Operation as name.
- KubeMQ: use the same serializers available in the framework.

By moving to the AddHttpClient capability on AspNetCore, an error was introduced regarding the usage of the JwtHttpHandler when used on a OAuth2 or OpenID Connect scenario.
The user context in those 2 scenarios are available on the HttpContext => It is important to understand that this is only happening when a call is done Rest call.
When a httpClient is created an the Bearer Token of the user must be injected, the only way to retrieve this is by accessing the user context via the IHttpAccesssor implementation.
This is why for those 2 cases, the constructor has been changed and support now the IHttpAccessor parameter. During the call, the user context will be retrieve (if exists) and be used
to inject the bearer token.

For gRPC, an error was discovered during server streaming and the ClientErrorInterceptor was reading the data without giving this back in the pipeline.

In EfCore, the change tracker implementation was used to update the EF Tracking based on the IdEntity<T> to set the EF Core context to Update, Delete, insert. The problem with this
implementation is with Navigation (obect with an ICollection<T>) and a different kind of T (Guid and integer for example). As the ChangeTracker was based on a specific T, Navigation was impossible.
To be get rid off this, the implementation is now based at the PersitEntity base class.

Blazor.
Two exension methods are now available. One is used to set the culture of the authenticated user (via his/her current culture in the token) or with the user action to do it.
The second one is to add new Policy based on the Operation name that a user has.

KubeMQ serializer was before fixed (Protobuf) and it is now using the same serializer factory that caching. So with the standard implementation exising in the framework: System.Text.Json and Protobuf are possible.

#### 5.0.6.1
24 June 2021.

- KubeMQ 2.2.8 features.
- ISerializationFactory is removed.
- Add JsonSerialiser based on System.Text.Json.
- Refactoring of JwtHttpHandler => no need of HttpHandler.
- Add new OboTokenProvider.
- Add new OboClientTokenProvider.

Previously to use the serializer, it was necessary to create a ISerializerFactory to create the serializer. 
Now you have simply to inject the serializer you want.
The default serializer is defined with no name.
</br>Use a named registration if you want to define a specific one.

```csharp
// Default
container.Register<IObjectSerialization, ProtoBufSerialization>();
// Named one.
container.Register<IObjectSerialization, JsonSerialization>("json");
```
JwtHttpHandler refactoring.
As the HttpClient is not created based on the IHttpClientFactory and services configuration, the HttpHandler are defined 
by deriving from JwtHtppHandler. 

```csharp
            services.AddHttpClient("OAuth")
                .AddHttpMessageHandler<OAuthHttpHandler>()
                .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(200)));
```
The Default end HttpHandler is added automatically or defined specifically when defining the AddHttpClient.

New Adfs and Azure ObotokenProvider.
Implement the token provider that will create based on the OAuth settings of the service and the ServiceApplicationId of the targeting service a JwtBearerToken.

New OboClientTokenProvider.
Is intended to be used only on UI code and it will call an endpoint defined on the Yarp facade service that will generate a bearer token
to call the targeting services. The token is generated from the service and stored on the UI application in memory. When the
token is expired a new call is done to the backend to generate a new one.




#### 5.0.6
21 May 2021

Update the packages to the AspNetCore 5.0.6.

####  5.0.5
20 May 2021.

Release of Arc4u based on AspNetCore 5.0.5.



