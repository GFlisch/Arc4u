using System;
using System.Collections.Generic;
using System.Linq;

namespace Arc4u.Standard.Diagnostics.AspNetCore
{
    static class ExceptionPropertyValues
    {
        /// <summary>
        /// Get the property values of an object (which is really a <see cref="Exception"/> in our case) transformed in a way that serialization using Json will be able to handle.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetSerializable(object obj)
        {
            return GetSerializable(obj, new HashSet<object>());
        }

        private static object GetSerializable(object obj, HashSet<object> visited)
        {
            if (obj is null)
                return null;
            var type = obj.GetType();
            try
            {
                if (IsSimpleType(type))
                    return obj;
                // special case for Type and all derivates: just serialize their name
                if (obj is Type t)
                    return t.Name;
                // avoid endless loops for reference types. This is unlikely to occur in exception instances, but one never knows.
                if (visited.Add(obj))
                {
                    // dictionaries with strings are special in Json, they have a compact serialization format
                    if (type.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == typeof(IDictionary<,>) || x.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)) && x.GetGenericArguments()[0] == typeof(string)))
                    {
                        //return Format((dynamic)obj, visited);
                        var dictionary = new Dictionary<string, object>();
                        foreach (var item in (System.Collections.IEnumerable)obj)
                        {
                            var itemType = item.GetType();
                            var key = itemType.GetProperty("Key").GetValue(item);
                            var value = itemType.GetProperty("Value").GetValue(item);
                            dictionary[key.ToString()] = GetSerializable(value, visited);
                        }
                        return dictionary;
                    }
                    else if (obj is System.Collections.IEnumerable enumerable)
                    {
                        // this will handle arrays and all collections
                        var collection = new List<object>();
                        foreach (var item in enumerable)
                            collection.Add(GetSerializable(item, visited));
                        return collection.ToArray();
                    }
                    else
                    {
                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        foreach (var property in type.GetProperties())
                        {
                            // we don't want to deserialize Exception.TargetSize, since it's huge and brings no benefit
                            if (property.DeclaringType == typeof(Exception) && property.Name == nameof(Exception.TargetSite))
                                continue;
                            var value = property.GetValue(obj);
                            properties[property.Name] = GetSerializable(value, visited);
                        }
                        return properties;
                    }
                }
                else
                    return "Instance already serialized";
            }
            catch
            {
                return $"Unable to serialize {type.Name}";
            }
        }

        /// <summary>
        /// Format a dictionary with strings as keys into something that Json recognizes as a structure
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        private static Dictionary<string, object> Format<TKey, TValue>(IEnumerable<KeyValuePair<string, TValue>> dictionary, HashSet<object> visited)
        {
            var elements = new Dictionary<string, object>();
            foreach (var item in dictionary)
                elements[item.Key] = GetSerializable(item.Value, visited);
            return elements;
        }


        /// <summary>
        /// Return true if the <paramref name="type"/> is simple enough and requires no transformation to be serializable by default.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsSimpleType(Type type)
        {
            // Nullables are simple if their underlying type is simple
            if (type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            // enums and other simple things are OK
            if (type.IsEnum || type == typeof(Guid) || type == typeof(DateTimeOffset))
                return true;
            var typeCode = Type.GetTypeCode(type);
            if (typeCode == TypeCode.Empty || typeCode == TypeCode.Object)
                return false;
            // all the other type codes are things that Json can serialize by default.
            return true;
        }
    }
}
