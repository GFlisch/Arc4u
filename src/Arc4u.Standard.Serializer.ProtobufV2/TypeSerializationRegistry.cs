using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Arc4u.Serializer.Protobuf
{
    /// <summary>
    /// A registry of types that need special treatment for serialization. This does not expose the actual serialization method (protobuf).
    /// </summary>
    public static class TypeSerializationRegistry
    {
        private interface IMetaTypeCommand
        {
            void ApplyTo(MetaType metaType);
        }

        /// <summary>
        /// Set a surrogate for a type
        /// </summary>
        private class SurrugateCommand : IMetaTypeCommand
        {
            private readonly Type _surrogateType;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="surrogateType">
            /// This is the surrogate type to use when serializing/deserializing this type.
            /// The surrogate type needs to implement 2 implicit conversion operators which will be called when (de)serializing
            ///     public static implicit operator [SurrogateType]([Type] value)
            ///     public static implicit operator [Type]([SurrogateType] value)
            /// </param>
            public SurrugateCommand(Type surrogateType)
            {
                _surrogateType = surrogateType;
            }

            public void ApplyTo(MetaType metaType)
            {
                metaType.SetSurrogate(_surrogateType);
            }
        }

        private class FactoryMethodInfoCommand : IMetaTypeCommand
        {
            private readonly MethodInfo _factory;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="factory">the factory method (a static method of the type returning an instance of that type)</param>
            public FactoryMethodInfoCommand(MethodInfo factory)
            {
                /// Protobuf will do its own validation, but only when <see cref="Register(RuntimeTypeModel)"/> is called.
                /// By doing some of it here, we avoid the confusion of delayed exceptions.
                if (!factory.IsStatic)
                    throw new ArgumentException($"The factory method for {factory.ReturnType} must be static");
                _factory = factory;
            }

            public void ApplyTo(MetaType metaType)
            {
                metaType.SetFactory(_factory);
            }
        }


        private class FactoryNameCommand : IMetaTypeCommand
        {
            private readonly string _factory;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="factory">the name of the factory method (a static method of the type returning an instance of that type)</param>
            public FactoryNameCommand(string factory)
            {
                _factory = factory;
            }

            public void ApplyTo(MetaType metaType)
            {
                metaType.SetFactory(_factory);
            }
        }


        private class AddFieldCommand : IMetaTypeCommand
        {
            private readonly int _fieldNumber;
            private readonly string _memberName;
            private readonly object _defaultValue;

            public AddFieldCommand(int fieldNumber, string memberName, object defaultValue = null)
            {
                _fieldNumber = fieldNumber;
                _memberName = memberName;
                _defaultValue = defaultValue;
            }

            public void ApplyTo(MetaType metaType)
            {
                if (_defaultValue == null)
                    metaType.Add(_fieldNumber, _memberName);
                else
                    metaType.Add(_fieldNumber, _memberName, _defaultValue);
            }
        }

        /// <summary>
        /// Container metadata info for the type, which is then applied to the type model.
        /// </summary>
        private class MetaTypeCommands : IMetaTypeCommand
        {
            /// <summary>
            /// True if default behaviour still needs to be applied on the type. Defaults to false
            /// </summary>
            public readonly bool ApplyDefaultBehaviour;

            private readonly List<IMetaTypeCommand> _commands;

            public MetaTypeCommands(bool applyDefaultBehaviour)
            {
                ApplyDefaultBehaviour = applyDefaultBehaviour;
                _commands = new List<IMetaTypeCommand>();
            }

            public void Add(IMetaTypeCommand command)
            {
                lock (_commands)
                    _commands.Add(command);
            }

            public void ApplyTo(MetaType metaType)
            {
                lock (_commands)
                    foreach (var command in _commands)
                        command.ApplyTo(metaType);
            }
        }

        /// <summary>
        /// The registry of types to be associated with specific metatype information.
        /// </summary>
        private static readonly Dictionary<Type, MetaTypeCommands> _registry = new Dictionary<Type, MetaTypeCommands>();

        /// <summary>
        /// Return the metatype info associated with a type, create it if necessary.
        /// </summary>
        /// <param name="type">the type to associate the metatype information with</param>
        /// <param name="applyDefaultBehaviour">true if default behavior should still be applied to the type</param>
        /// <returns></returns>
        private static MetaTypeCommands GetMetaTypeCommands(Type type, bool applyDefaultBehaviour = false)
        {
            MetaTypeCommands metaTypeInfo;
            lock (_registry)
                if (!_registry.TryGetValue(type, out metaTypeInfo))
                    _registry.Add(type, metaTypeInfo = new MetaTypeCommands(applyDefaultBehaviour));
            return metaTypeInfo;
        }

        /// <summary>
        /// Define surrogates type for a type.
        /// </summary>
        /// <param name="type">the type to map to a surrogate</param>
        /// <param name="surrogateType">
        /// The surrogate type, which needs to implement 2 implicit conversion operators which will be called when (de)serializing
        ///     public static implicit operator [SurrogateType]([Type] value)
        ///     public static implicit operator [Type]([SurrogateType] value)
        /// </param>
        public static void SetSurrogate(Type type, Type surrogateType)
        {
            GetMetaTypeCommands(type).Add(new SurrugateCommand(surrogateType));
        }

        /// <summary>
        /// Set a factory method for the type, to be called when the deserializer needs an instance of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="factory">the name of the public static parameterless method returning an object of <paramref name="type"/></param>
        public static void SetFactory(Type type, string factory)
        {
            GetMetaTypeCommands(type).Add(new FactoryNameCommand(factory));
        }

        /// <summary>
        /// Set a factory method for the type, to be called when the deserializer needs an instance of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="factory">the method info of the public static parameterless method returning an object of <paramref name="type"/></param>
        public static void SetFactory(Type type, MethodInfo factory)
        {
            GetMetaTypeCommands(type).Add(new FactoryMethodInfoCommand(factory));
        }

        /// <summary>
        /// Set a factory method for the type, to be called when the deserializer needs an instance of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">the static delegate to use to return instances of the provided type</param>
        public static void SetFactory<T>(Func<T> factory)
        {
            GetMetaTypeCommands(typeof(T)).Add(new FactoryMethodInfoCommand(factory.Method));
        }

        /// <summary>
        /// Add a serializable field to the object explicitly, with an optional default value.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldNumber">the 0-starting field number for the field</param>
        /// <param name="memberName">the name of the member to use to (de)serialize</param>
        public static void AddField(Type type, int fieldNumber, string memberName)
        {
            GetMetaTypeCommands(type).Add(new AddFieldCommand(fieldNumber, memberName));
        }

        /// <summary>
        /// Add a serializable field to the object explicitly, with an optional default value.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldNumber">the 0-starting field number for the field</param>
        /// <param name="memberName">the name of the member to use to (de)serialize</param>
        /// <param name="defaultValue">an optional default value</param>
        public static void AddField(Type type, int fieldNumber, string memberName, object defaultValue)
        {
            GetMetaTypeCommands(type).Add(new AddFieldCommand(fieldNumber, memberName, defaultValue));
        }

        /// <summary>
        /// Apply the metatype information to Protobuf's <see cref="RuntimeTypeModel"/>
        /// </summary>
        /// <param name="runtimeTypeModel"></param>
        internal static void Register(RuntimeTypeModel runtimeTypeModel)
        {
            lock (_registry)
                foreach (var item in _registry)
                {
                    var metaType = runtimeTypeModel.Add(item.Key, item.Value.ApplyDefaultBehaviour);
                    item.Value.ApplyTo(metaType);
                }
        }
    }
}
