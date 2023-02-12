using System;
using Arc4u.KubeMQ.AspNetCore.Configuration;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using EventsStoreType = KubeMQ.SDK.csharp.Subscription.EventsStoreType;

namespace Microsoft.Extensions.DependencyInjection;

public class KubeMQConfigurationExtension
{
    public KubeMQConfigurationExtension(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
        _defaultParameter = new DefaultParameter();
        configuration.Bind("KubeMQ:Default", _defaultParameter);
    }

    public KubeMQConfigurationExtension(IServiceCollection services, IConfiguration configuration, Action<DefaultParameter> options)
    {
        _services = services;
        _configuration = configuration;
        _defaultParameter = new DefaultParameter();
        options.Invoke(_defaultParameter);
    }

    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly DefaultParameter _defaultParameter;

    #region Queue

    void SetDefaultValue(ChannelParameter channel)
    {
        channel.Address = String.IsNullOrWhiteSpace(channel.Address) ? _defaultParameter.Address : channel.Address;
        channel.Identifier = String.IsNullOrWhiteSpace(channel.Identifier) ? _defaultParameter.Identifier : channel.Identifier;
    }

    #region AddListenerToQueue

    public HandlerQueueForSyntax AddListenerToQueue(Action<QueueParameters> options)
    {
        var parameter = new MessageQueueParameters();
        options.Invoke(parameter);
        SetDefaultValue(parameter);
        _services.AddSingleton<QueueParameters>(parameter);

        return new HandlerQueueForSyntax(_services, _configuration, parameter);
    }

    public HandlerQueueForSyntax AddListenerToQueue(string section, Func<PolicyBuilder, RetryPolicy> customPolicy = null)
    {
        var parameter = new MessageQueueParameters();
        _configuration.GetSection(section).Bind(parameter);
        if (null != customPolicy) parameter.RetryPolicy = customPolicy;
        SetDefaultValue(parameter);
        _services.AddSingleton<QueueParameters>(parameter);

        return new HandlerQueueForSyntax(_services, _configuration, parameter);
    }

    #endregion

    #region SendMessageToQueue
    public MessageQueueMapperForSyntaxExtension SendMessageToQueue(string section, Func<PolicyBuilder, RetryPolicy> customRetryPolicy = null)
    {
        var parameter = new QueueConfiguration();
        _configuration.GetSection(section).Bind(parameter);
        SetDefaultValue(parameter);
        if (null != customRetryPolicy) parameter.RetryPolicy = customRetryPolicy;

        return new MessageQueueMapperForSyntaxExtension(_services, _configuration, parameter);
    }

    public MessageQueueMapperForSyntaxExtension SendMessageToQueue(Action<QueueConfiguration> options)
    {
        var parameter = new QueueConfiguration();
        options.Invoke(parameter);
        SetDefaultValue(parameter);

        return new MessageQueueMapperForSyntaxExtension(_services, _configuration, parameter);
    }
    #endregion

    #region SendMessageToPersistedQueue
    public MessageQueueMapperForSyntaxExtension SendMessageToPersistedQueue(Action<PersistedQueueConfiguration> options)
    {
        var parameter = new PersistedQueueConfiguration();
        options.Invoke(parameter);
        SetDefaultValue(parameter);

        return new MessageQueueMapperForSyntaxExtension(_services, _configuration, parameter);
    }

    public MessageQueueMapperForSyntaxExtension SendMessageToPersistedQueue(string section, Func<PolicyBuilder, RetryPolicy> customRetryPolicy = null)
    {
        var parameter = new PersistedQueueConfiguration();
        _configuration.GetSection(section).Bind(parameter);
        if (null != customRetryPolicy) parameter.RetryPolicy = customRetryPolicy;
        SetDefaultValue(parameter);

        return new MessageQueueMapperForSyntaxExtension(_services, _configuration, parameter);
    }
    #endregion

    #endregion

    #region PubSub

    public PubSubMapperForSyntaxExtension PublishEventTo(Action<PubSubConfiguration> options)
    {
        var parameter = new PubSubConfiguration();
        options.Invoke(parameter);
        SetDefaultValue(parameter);

        return new PubSubMapperForSyntaxExtension(_services, _configuration, parameter);
    }

    public PubSubMapperForSyntaxExtension PublishEventTo(string section)
    {
        var parameter = new PubSubConfiguration();
        _configuration.GetSection(section).Bind(parameter);
        SetDefaultValue(parameter);

        return new PubSubMapperForSyntaxExtension(_services, _configuration, parameter);
    }

    public PubSubMapperForSyntaxExtension PublishPersistedEventTo(Action<PersistedPubSubConfiguration> options)
    {
        var parameter = new PersistedPubSubConfiguration();
        options.Invoke(parameter);
        SetDefaultValue(parameter);

        return new PubSubMapperForSyntaxExtension(_services, _configuration, parameter);
    }

    public PubSubMapperForSyntaxExtension PublishPersistedEventTo(string section)
    {
        var parameter = new PersistedPubSubConfiguration();
        _configuration.GetSection(section).Bind(parameter);
        SetDefaultValue(parameter);

        return new PubSubMapperForSyntaxExtension(_services, _configuration, parameter);
    }

    #region AddListenerToPubSub

    public HandlerPubSubForSyntax AddListenerToPubSub(Action<SubscriberParameters> options)
    {
        var parameter = new SubscriberParameters();
        options.Invoke(parameter);
        SetDefaultValue(parameter);
        ThrowIfNotValid(parameter);

        parameter.GroupName = String.IsNullOrWhiteSpace(parameter.GroupName) ? parameter.Identifier : parameter.GroupName;

        _services.AddSingleton<SubscriberParameters>(parameter);

        return new HandlerPubSubForSyntax(_services, _configuration, parameter);
    }

    public HandlerPubSubForSyntax AddListenerToPubSub(string section, Func<PolicyBuilder, RetryPolicy> customPolicy = null)
    {
        var parameter = new SubscriberParameters();
        _configuration.GetSection(section).Bind(parameter);
        SetDefaultValue(parameter);
        if (null != customPolicy) parameter.RetryPolicy = customPolicy;
        ThrowIfNotValid(parameter);

        parameter.GroupName = String.IsNullOrWhiteSpace(parameter.GroupName) ? parameter.Identifier : parameter.GroupName;

        _services.AddSingleton<SubscriberParameters>(parameter);

        return new HandlerPubSubForSyntax(_services, _configuration, parameter);
    }

    #endregion

    #region AddListenerToPersistedPubSub

    public HandlerPubSubForSyntax AddListenerToPersistedPubSub(Action<PersistedSubscriberParameters> options)
    {
        var parameter = new PersistedSubscriberParameters();
        options.Invoke(parameter);
        SetDefaultValue(parameter);
        ThrowIfNotValid(parameter);

        parameter.GroupName = String.IsNullOrWhiteSpace(parameter.GroupName) ? parameter.Identifier : parameter.GroupName;

        _services.AddSingleton<SubscriberParameters>(parameter);

        return new HandlerPubSubForSyntax(_services, _configuration, parameter);
    }

    public HandlerPubSubForSyntax AddListenerToPersistedPubSub(string section, Func<PolicyBuilder, RetryPolicy> customPolicy = null)
    {
        var parameter = new PersistedSubscriberParameters();
        _configuration.GetSection(section).Bind(parameter);
        SetDefaultValue(parameter);
        if (null != customPolicy) parameter.RetryPolicy = customPolicy;
        ThrowIfNotValid(parameter);

        parameter.GroupName = String.IsNullOrWhiteSpace(parameter.GroupName) ? parameter.Identifier : parameter.GroupName;

        _services.AddSingleton<SubscriberParameters>(parameter);

        return new HandlerPubSubForSyntax(_services, _configuration, parameter);
    }

    #endregion

    #region AddListenerToPubSubByInstance

    public HandlerPubSubForSyntax AddListenerToPubSubByInstance(Action<SubscriberParameters> options)
    {
        var parameter = new SubscriberParameters();
        options.Invoke(parameter);
        SetDefaultValue(parameter);
        ThrowIfNotValid(parameter);

        parameter.GroupName = String.Empty;

        _services.AddSingleton<SubscriberParameters>(parameter);

        return new HandlerPubSubForSyntax(_services, _configuration, parameter);
    }

    public HandlerPubSubForSyntax AddListenerToPubSubByInstance(string section, Func<PolicyBuilder, RetryPolicy> customPolicy = null)
    {
        var parameter = new SubscriberParameters();
        _configuration.GetSection(section).Bind(parameter);
        SetDefaultValue(parameter);
        if (null != customPolicy) parameter.RetryPolicy = customPolicy;
        ThrowIfNotValid(parameter);

        parameter.GroupName = String.Empty;

        _services.AddSingleton<SubscriberParameters>(parameter);

        return new HandlerPubSubForSyntax(_services, _configuration, parameter);
    }

    #endregion

    #region AddListenerToPersistedPubSubByInstance

    public HandlerPubSubForSyntax AddListenerToPersistedPubSubByInstance(Action<PersistedSubscriberParameters> options)
    {
        var parameter = new PersistedSubscriberParameters();
        options.Invoke(parameter);
        SetDefaultValue(parameter);

        ThrowIfNotValid(parameter);

        parameter.GroupName = String.Empty;

        _services.AddSingleton<SubscriberParameters>(parameter);

        return new HandlerPubSubForSyntax(_services, _configuration, parameter);
    }

    public HandlerPubSubForSyntax AddListenerToPersistedPubSubByInstance(string section, Func<PolicyBuilder, RetryPolicy> customPolicy = null)
    {
        var parameter = new PersistedSubscriberParameters();
        _configuration.GetSection(section).Bind(parameter);
        SetDefaultValue(parameter);
        if (null != customPolicy) parameter.RetryPolicy = customPolicy;
        ThrowIfNotValid(parameter);

        parameter.GroupName = String.Empty;

        _services.AddSingleton<SubscriberParameters>(parameter);

        return new HandlerPubSubForSyntax(_services, _configuration, parameter);
    }

    #endregion

    private void ThrowIfNotValid(PersistedSubscriberParameters subscriberParameters)
    {
        ThrowIfNotValid((SubscriberParameters)subscriberParameters);

        if (EventsStoreType.Undefined == subscriberParameters.EventsStoreType)
        {
            throw new NotSupportedException("Undefined is not supported for persisted events store.");
        }
    }

    private void ThrowIfNotValid(SubscriberParameters subscriberParameters)
    {
        if (String.IsNullOrWhiteSpace(subscriberParameters.Address))
            throw new ArgumentNullException(nameof(subscriberParameters.Address));

        if (String.IsNullOrWhiteSpace(subscriberParameters.Identifier))
            throw new ArgumentNullException(nameof(subscriberParameters.Identifier));

        if (String.IsNullOrWhiteSpace(subscriberParameters.Namespace))
            throw new ArgumentNullException(nameof(subscriberParameters.Namespace));

    }

    #endregion
}
