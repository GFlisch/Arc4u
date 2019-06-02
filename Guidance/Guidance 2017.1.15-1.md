June 2019.

# Guidance 2017.1.15-1

This is an update of the 2017.1.15-0 version of the Guidance.

What is in this update:

- fixes:

  - Adding a database project: Entity framework in the web.config file is not added.
  - Create solution: when Wcf is selected, the register types section was not updated.
  - Adding a certificate service account, storeLocation was not taken into action.
  - NServiceBus certificate integration.
  - Change from Messages to Messaging project and solve creation of a subfolder BE. 

- New:

  - NServiceBus messageScope integration in Hangfire jobs and Web Api facade if NServiceBus exists.

## Fixes

### Entity framework

The Entity framework package was not added on the IISHost project, so the web.config section was not added.

### Wcf register types

The following types were not added during the creation of a project (Server Wcf mode).

``` xml
    <add type="Arc4u.ServiceModel.Aspect.AspectMethodExecution, Arc4u.ServiceModel"/>
    <add type="Arc4u.ServiceModel.Aspect.AspectMethodNotSettingCultureInfoExecution, Arc4u.ServiceModel"/>
    <add type="Arc4u.ServiceModel.Aspect.AspectLogParametersMethodExecution, Arc4u.ServiceModel"/>
    <add type="Arc4u.ServiceModel.Aspect.AspectLogParametersMethodNotSettingCultureInfoExecution, Arc4u.ServiceModel"/>
```

### Certificate service account

When creating a service account for jobs, the CurrentUser storeLocation was not taken into account => the localhost configuration must read the certificate from the CurrentUser location but the other must read this from LocalMachine.

### NServiceBus certificate integration

The 15-0 version doesn't integrate certificate authentication when connecting RabbitMQ. This is now fixed.

This feature is linked to an update of Arc4u: 4.6.0.11. So you need to update your current project to this version of Arc4u to use this.

What's changed:

  - Connection string doesn't contains the username password information but the useTls=true.
  - The default virutal host is set to '/'. So don't forget to change it to your defined virtual host.
  - Create a RabbitMQ certificate settings section in the config file and the code to read it.
  - Inject this settings section in the SenderConfigurationEndpoint.
  - Use the extension method defined in the Arc4u.NServiceBus.RabbitMQ: transport.SetCertificateAuthentication(certificateSettings);
 

  ``` csharp

    [Export("SenderEndpoint", typeof(IEndpointConfiguration))]
    public class SenderConfigurationEndpoint : SenderEndpointConfigBase
    {
		[ImportingConstructor]
        public SenderConfigurationEndpoint([Import("Rabbitmq")] IAppSettings settings)
        {
            certificateSettings = settings;
        }

        readonly IAppSettings certificateSettings;

        ...

        public override Task ConfigureTransportAndRoutingAsync(EndpointConfiguration endpointConfiguration)
        {
            TransportExtensions<RabbitMQTransport> transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(ConfigurationManager.ConnectionStrings["NServiceBus/Transport"].ConnectionString);
            transport.UsePublisherConfirms(true);
            transport.UseConventionalRoutingTopology();
            transport.SetCertificateAuthentication(certificateSettings);

            ...
  ```

## New feature

  When you will add an Hangfire job or an Handler and you have added NServiceBus, the code will reflect this and add the following code.

  ``` csharp
            using (var scope = new MessagesScope("SenderEndpoint"))
            {
                // Call your Business layer.
                var result = bl.DoYourJob();

                scope.Complete();

                return Ok(result);
            }
  ```

>! Important
> The previous code you wrote, didn't contain the MessageScope and you have to update your code in
> all your facade, interface and jobs!

