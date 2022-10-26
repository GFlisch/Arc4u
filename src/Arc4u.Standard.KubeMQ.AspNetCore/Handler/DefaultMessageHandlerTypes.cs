using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.KubeMQ.AspNetCore.Handler;
using Arc4u.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;


namespace Arc4u.KubeMQ.AspNetCore
{
    public class DefaultMessageHandlerTypes : IMessageHandlerTypes
    {
        ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        public DefaultMessageHandlerTypes(IOptionsMonitor<HandlerDefintion> definitions, ILogger<DefaultMessageHandlerTypes> logger, IServiceProvider serviceProvider)
        {
            _definitions = definitions;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        private readonly ILogger<DefaultMessageHandlerTypes> _logger;
        private readonly IOptionsMonitor<HandlerDefintion> _definitions;
        private readonly IServiceProvider _serviceProvider;
        private object locker = new Object();

        public Type GetOrAddType(string keyName, out Type handlerDataType)
        {
            if (!_types.ContainsKey(keyName))
            {
                lock (locker)
                {
                    // We do the lock only if needed and we have only one thread doing the initialization.
                    if (!_types.ContainsKey(keyName))
                    {
                        var handlerDefinition = _definitions.Get(keyName);

                        if (null == handlerDefinition.HandlerType)
                        {
                            _logger.Technical().Error($"No defintion found for {keyName}").Log();
                            throw new NullReferenceException($"No handler data type is registered for the key {keyName}.");
                        }

                        Console.WriteLine("Regster the handler in the collection");
                        var typeHandler = typeof(IMessageHandler<>);
                        var types = new Type[] { handlerDefinition.HandlerType };
                        _types.TryAdd(keyName, typeHandler.MakeGenericType(types));
                    }

                }

            }

            var interfaceType = _types[keyName];
            handlerDataType = interfaceType.GetGenericArguments()[0];

            return interfaceType;
        }

        public IObjectSerialization Serializer(string keyName)
        {
            var handlerDefinition = _definitions.Get(keyName);

            if (null == handlerDefinition.Serializer || !_serviceProvider.TryGetService<IObjectSerialization>(handlerDefinition.Serializer, out var serializerFactory))
            {
                return _serviceProvider.GetService<IObjectSerialization>();
            }

            return serializerFactory;
        }

    }
}
