using ProtoBuf.Meta;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Arc4u.Serializer.ProtoBuf
{
    /// <summary>
    /// Update a <see cref="RuntimeTypeModel"/> with a type, even if that type doesn't contain a <see cref="DataContractAttribute"/>
    /// </summary>
    sealed class RuntimeTypeModelUpdater
    {
        private readonly RuntimeTypeModel _model;
        private readonly Dictionary<Type, HashSet<Type>> _subTypes;
        private readonly ConcurrentDictionary<Type, bool> _builtTypes;
        private readonly object _lock;

        private static readonly Type[] ComplexPrimitives = new[] { typeof(object), typeof(ValueType), typeof(Enum), typeof(Array) };
        private static readonly HashSet<Type> _protobuf_builtints = new HashSet<Type> { typeof(DateTime), typeof(string), typeof(decimal), typeof(Guid), typeof(TimeSpan), typeof(Uri), typeof(byte) };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">the runtime model to update. Existing types already added to the model will be considered as built-in</param>
        public RuntimeTypeModelUpdater(RuntimeTypeModel model)
        {
            _model = model;
            _lock = new object();
            _subTypes = new Dictionary<Type, HashSet<Type>>();
            _builtTypes = new ConcurrentDictionary<Type, bool>();
            // any types that the model already defines need to be considered as built-in.
            // this is important starting from V3 of protobuf-net
            foreach (MetaType metaType in model.GetTypes())
                _builtTypes.TryAdd(metaType.Type, false);
            /// starting from protobuf-net V3, the following built-in types need to be added explicitly. Attempts to check them explicitly using <see cref="TypeModel.CanSerializeBasicType(Type)"/> yields unwanted side effects
            /// by silently registering the type argument.
            _builtTypes.TryAdd(typeof(DateTime), false);
            _builtTypes.TryAdd(typeof(string), false);
            _builtTypes.TryAdd(typeof(decimal), false);
            _builtTypes.TryAdd(typeof(Guid), false);
            _builtTypes.TryAdd(typeof(TimeSpan), false);
            _builtTypes.TryAdd(typeof(Uri), false);
            _builtTypes.TryAdd(typeof(byte), false);
        }

        private bool IsBuiltInType(Type type)
        {
            if (_builtTypes.ContainsKey(type))
                return true;
            if (_protobuf_builtints.Contains(type))
                return true;
            if (type.IsArray && _protobuf_builtints.Contains(type.GetElementType()))
                return true;
            return false;
        }

        /// <summary>
        /// Make sure to update the model so that it knows how to serialize <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        public void Update(Type type)
        {
            if (IsBuiltInType(type))
                return;
            lock (_lock)
            {
                if (IsBuiltInType(type))
                    return;
                var all = GetGraph(type).OrderBy(t => t.FullName);
                var all0 = all.ToList();
                foreach (var t in all0)
                    InternalBuild(t);
            }
        }

        private void InternalBuild(Type type)
        {
            if (IsKnownSerializable(type))
                return;
            _builtTypes.TryAdd(type, false);
            FlatBuild(type);
            EnsureBaseClasses(type);
            EnsureGenerics(type);
        }

        /// <summary>
        /// Return true if the provided type is primitive (in which case it is de-facto serializable) or already built in the model.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsPrimitiveOrKnown(Type type)
        {
            return type.IsPrimitive || type.IsEnum || IsBuiltInType(type) || ComplexPrimitives.Contains(type);
        }


        /// <summary>
        /// Return true if the type is already known to be serializable to the model.
        /// It differs from <see cref="IsPrimitiveOrKnown(Type)"/> in that it also handles nullable value types.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsKnownSerializable(Type type)
        {
            if (type == null)
                return true;
            if (IsPrimitiveOrKnown(type))
                return true;
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null && IsPrimitiveOrKnown(underlyingType);
        }


        private static IEnumerable<Type> GetGraph(Type type)
        {
            var visited = new HashSet<Type>();
            var stack = new Stack<Type>();
            stack.Push(type);
            while (stack.Count > 0)
            {
                var i = stack.Pop();
                yield return i;
                visited.Add(i);
                var children = GetConnections(i);
                if (children != null)
                    foreach (var child in children)
                        if (!visited.Contains(child))
                            stack.Push(child);
            }
        }

        private static Type GetParent(Type type)
        {
            return type.BaseType;
        }

        private static IEnumerable<Type> GetChildren(Type type)
        {
            foreach (var knownType in type.GetCustomAttributes<KnownTypeAttribute>())
                if (knownType.Type != null)
                    yield return knownType.Type;
                else if (!string.IsNullOrWhiteSpace(knownType.MethodName))
                {
                    var method = type.GetMethod(knownType.MethodName);
                    if (method != null && method.IsStatic)
                    {
                        var types = (IEnumerable<Type>)method.Invoke(null, new object[0]);
                        foreach (var ktype in types)
                            yield return ktype;
                    }
                }
            var members = GetSerializableMembers(type);
            foreach (var member in members)
                yield return GetMemberType(member);
        }

        private static IEnumerable<Type> GetConnections(Type type)
        {
            var parent = GetParent(type);
            if (parent != null)
                yield return parent;
            var children = GetChildren(type);
            if (children != null)
                foreach (var c in children)
                    yield return c;
        }


#if false
        /// <summary>
        /// Return true if at least one of the members in <paramref name="memberInfos"/> are decorated with <see cref="DataMemberAttribute"/>
        /// </summary>
        /// <param name="memberInfos"></param>
        /// <returns></returns>
        private static bool ContainsSerializationAttribute(IEnumerable<MemberInfo> memberInfos)
        {
            foreach (var memberInfo in memberInfos)
                if (memberInfo.IsDefined(typeof(DataMemberAttribute)))
                    return true;
            return false;
        }
#endif

        /// <summary>
        /// Return true if the <paramref name="metaType"/> already defines a field for the specified <paramref name="memberInfo"/>
        /// </summary>
        /// <param name="metaType"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        private static bool ContainsMemberInfo(MetaType metaType, MemberInfo memberInfo)
        {
            foreach (var field in metaType.GetFields())
                if (field.Member == memberInfo)
                    return true;
            return false;
        }


        private void FlatBuild(Type type)
        {
            if (type.IsAbstract)
                return;

            // for some reason, the "applyDefaultBehaviour" parameter is ignored and all members decorated with the appropriate attributes are already
            // added to the metatype. If we ignore this, every field will be added twice, which is bad for collections since deserializing them
            // will make them double in size.
            var meta = _model.Add(type, applyDefaultBehaviour: false);
            var members = GetSerializableMembers(type);
            // if at least one field or property has a serialization attribute, only serialize those with that attribute.
            // otherwise, serialize all the fields, public or private
            var unordered = new List<MemberInfo>();
            /// add the members in the order specified by <see cref="DataMemberAttribute.Order"/>, since the layout will be influenced by this.
            /// the members that do not have a valid order are sorted and appended after those explicitly ordered.
            foreach (var member in members)
                if (member.IsDefined(typeof(DataMemberAttribute)))
                {
                    if (!ContainsMemberInfo(meta, member))
                    {
                        var dataMember = (DataMemberAttribute)member.GetCustomAttribute(typeof(DataMemberAttribute));
                        if (dataMember.Order >= 0)
                            meta.Add(dataMember.Order, member.Name);
                        else
                            unordered.Add(member);
                    }
                }
                else if (member.IsDefined(typeof(XmlElementAttribute)))
                {
                    if (!ContainsMemberInfo(meta, member))
                    {
                        var dataMember = (XmlElementAttribute)member.GetCustomAttribute(typeof(XmlElementAttribute));
                        if (dataMember.Order >= 0)
                            meta.Add(dataMember.Order, member.Name);
                        else
                            unordered.Add(member);
                    }
                }
                else if (!ContainsMemberInfo(meta, member))
                    unordered.Add(member);
            // add all the unordered fields in alphabetical order. It looks like this is what ProtoBuf.net does as well
            if (unordered.Count > 0)
            {
                unordered.Sort((x, y) => StringComparer.Ordinal.Compare(x.Name, y.Name));
                foreach (var memberInfo in unordered)
                    meta.Add(memberInfo.Name);
            }
            // this is the equivalent of SkipConstructor, which follows the standard behavior at https://docs.microsoft.com/en-us/dotnet/standard/serialization/serialization-guidelines
            meta.UseConstructor = false;
            foreach (var memberType in members.Select(m => GetMemberType(m)).Where(t => !t.IsPrimitive))
                InternalBuild(memberType);
        }

        /// <summary>
        /// Return true if the <paramref name="memberInfo"/> contains an attribute signalling it should not be a part of serialization
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        private static bool ContainsIgnoreAttribute(MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(IgnoreDataMemberAttribute)) || memberInfo.IsDefined(typeof(XmlIgnoreAttribute));
        }

        /// <summary>
        /// Returns true if the <paramref name="memberInfo"/> contains an attribute signalling it should be serialized
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        private static bool ContainsSerializeMemberAttribute(MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(DataMemberAttribute)) || memberInfo.IsDefined(typeof(XmlElementAttribute));
        }

        private static IEnumerable<MemberInfo> GetSerializableMembers(Type type)
        {
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var serializableMembers = new List<MemberInfo>();
            serializableMembers.AddRange(type.GetFields(Flags).Where(x => ContainsSerializeMemberAttribute(x) && !ContainsIgnoreAttribute(x)));
            serializableMembers.AddRange(type.GetProperties(Flags).Where(x => ContainsSerializeMemberAttribute(x) && !ContainsIgnoreAttribute(x)));
            if (serializableMembers.Count == 0)
                /// we have a type with no <see cref="DataMemberAttribute"/> or <see cref="XmlElementAttribute"/>, we need
                /// to scan again and collect all public properties unless they need to be ignored.
                serializableMembers.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => !ContainsIgnoreAttribute(x)));
            return serializableMembers;
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;
            else if (memberInfo is FieldInfo fieldInfo)
                return fieldInfo.FieldType;
            else
                throw new ArgumentException($"Unknown MemberInfo type {memberInfo?.GetType().Name}", nameof(memberInfo));
        }


        private void EnsureBaseClasses(Type type)
        {
            var baseType = type.BaseType;
            var inheritingType = type;

            while (baseType != null && baseType != typeof(object))
            {
                if (!_subTypes.TryGetValue(baseType, out var baseTypeEntry))
                {
                    baseTypeEntry = new HashSet<Type>();
                    _subTypes.Add(baseType, baseTypeEntry);
                }

                if (!baseTypeEntry.Contains(inheritingType))
                {
                    InternalBuild(baseType);
                    _model[baseType].AddSubType(baseTypeEntry.Count + 500, inheritingType);
                    baseTypeEntry.Add(inheritingType);
                }

                inheritingType = baseType;
                baseType = baseType.BaseType;
            }
        }

        private void EnsureGenerics(Type type)
        {
            if (type.IsGenericType || (type.BaseType != null && type.BaseType.IsGenericType))
            {
                var generics = type.IsGenericType ? type.GetGenericArguments() : type.BaseType.GetGenericArguments();
                foreach (var generic in generics)
                    InternalBuild(generic);
            }
        }
    }
}
