﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Arc4u
{
    /// <summary>
    /// Helper class to specify and apply includes on a <see cref="IQueryable&gt;T&lt;"/> object.
    /// </summary>
    /// <typeparam name="T">A class of the domain model.</typeparam>
    [DataContract(Namespace = "uri://arc4u.graph", Name = "GraphOf{0}")]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Graph<T> where T : class
    {
        private const String StringTypeNotAllowed = "Including a type of String is not allowed when you try to define a graph path.";
        private const String StructTypeNotAllowed = "Including a struct type is not allowed when you try to define a graph path.";
        private const String OnlyLevelOneIsAllowed = "It is not allowed to check more than one level!";

        [DataMember(Name = "Includes")]
        private List<String> _includes;

        /// <summary>
        /// Create a Graph instance based on the typeof(T).
        /// </summary>
        public Graph()
        {
            _includes = new List<String>();
        }

        /// <summary>
        /// Create a Graph instance based on the typeof(T).
        /// </summary>
        /// <param name="paths">The list of initials includes you want to predefined!</param>
        public Graph(IEnumerable<string> paths)
        {
            if (null == paths)
                throw new ArgumentNullException("paths");

            ValidatePaths(paths);

            _includes = new List<String>(paths);
        }

        private void ValidatePaths(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                var type = typeof(T);
                var properties = path.Split('.');
                foreach (var property in properties)
                {
                    var propertyInfo = type.GetRuntimeProperty(property);
                    if (null == propertyInfo)
                        throw new MissingMemberException(String.Format("The property {0} does not exist!", property));

                    if (propertyInfo.PropertyType.IsArray)
                    {
                        type = propertyInfo.PropertyType.GetElementType();

                        if (type == typeof(String))
                            throw new MissingMemberException(String.Format("The property {0} is a simple type!", property));

                        if (!type.GetTypeInfo().IsClass)
                            throw new MissingMemberException(String.Format("The property {0} is a simple type!", property));

                        continue;
                    }

                    if (!propertyInfo.PropertyType.GetTypeInfo().ImplementedInterfaces.All(t => t != typeof(IEnumerable)))
                    {
                        if (!propertyInfo.PropertyType.GetTypeInfo().IsGenericType)
                            throw new MissingMemberException(String.Format("The property {0} is not a generic collection!", property));

                        var types = propertyInfo.PropertyType.GenericTypeArguments;
                        foreach (var t in types)
                        {
                            if (t == typeof(String))
                                throw new MissingMemberException(String.Format("The property {0} is a simple type!", property));

                            if (!t.GetTypeInfo().IsClass)
                                throw new MissingMemberException(String.Format("The property {0} is a simple type!", property));
                        }

                        if (types.Length > 1)
                            throw new MissingMemberException(String.Format("Simple Collection is allowed, property {0} has more than one type.", property));

                        type = types[0];
                        continue;
                    }


                    type = propertyInfo.PropertyType;

                    if (type == typeof(String))
                        throw new MissingMemberException(String.Format("The property {0} is a simple type!", property));

                    if (!type.GetTypeInfo().IsClass)
                        throw new MissingMemberException(String.Format("The property {0} is a simple type!", property));
                }
            }
        }

        private string DebuggerDisplay
        {
            get
            {
                var info = new StringBuilder();
                info.Append(String.Format("{0}: ", typeof(T).Name));
                var includes = Includes;
                if (includes.Count == 0)
                {
                    info.Append("- No records.");
                    return info.ToString();
                }

                if (Includes.Count == 1)
                    info.Append(includes[0]);
                else
                {
                    info.Append(includes[0]);
                    info.Append(", ");
                    for (int i = 1; i < includes.Count - 1; i++)
                    {
                        info.Append(String.Format("{0}, ", includes[i]));
                    }
                    info.Append(includes[includes.Count - 1]);
                }
                return info.ToString();
            }
        }


        /// <summary>
        /// Let you add new <see cref="MemberExpression"/> you want to include in the object's graph.
        /// </summary>
        /// <param name="path">The <see cref="MemberExpression"/> you want to check.</param>
        /// <returns>The </returns>
        public Graph<T> Include<TProperty>(Expression<Func<T, TProperty>> path) where TProperty : class
        {
            var stringPath = EvaluateExpression(path);

            if (!String.IsNullOrWhiteSpace(stringPath) && stringPath[0] == '.')
                stringPath = stringPath.Substring(1);

            if (!String.IsNullOrWhiteSpace(stringPath) && !_includes.Exists(i => i == stringPath))
                _includes.Add(stringPath);

            return this;

        }


        public static String EvaluateExpression(Expression path)
        {
            if (null == path)
                throw new ArgumentNullException("path");

            var stringPath = String.Empty;
            var memberExpression = path as MemberExpression;
            if (null != memberExpression)
            {
                // Check if type is System.String. because String is of type Class and the include allows to parse member of type Class. But we are not interested to see the String type.
                if (memberExpression.Type == typeof(String))
                    throw new MemberAccessException(StringTypeNotAllowed);

                if (memberExpression.Type.IsArray)
                {
                    if (memberExpression.Type.GetElementType() == typeof(String))
                        throw new MemberAccessException(StringTypeNotAllowed);

                    if (!memberExpression.Type.GetElementType().GetTypeInfo().IsClass)
                        throw new MemberAccessException(StructTypeNotAllowed);
                }
                if (!memberExpression.Type.GetTypeInfo().ImplementedInterfaces.All(t => t != typeof(IEnumerable)))
                {
                    var types = memberExpression.Type.GenericTypeArguments;

                    foreach (var type in types)
                    {
                        if (type == typeof(String))
                            throw new MemberAccessException(StringTypeNotAllowed);

                        if (!type.GetTypeInfo().IsClass)
                            throw new MemberAccessException(StructTypeNotAllowed);
                    }
                }

                var s = memberExpression.ToString();
                stringPath += s.Substring(s.IndexOf('.'));
            }
            else
            {
                var methodCallExpression = path as MethodCallExpression;

                if (null != methodCallExpression)
                {
                    stringPath = methodCallExpression.Arguments.Aggregate(stringPath, (current, argument) => current + EvaluateExpression(argument));
                }
                else
                {
                    var lambdaExpression = path as LambdaExpression;
                    if (lambdaExpression != null)
                    {
                        stringPath = EvaluateExpression(lambdaExpression.Body) + stringPath;
                    }
                }
            }
            return stringPath;
        }

        /// <summary>
        /// Let you check if a member defined in the path expression was included in the object's graph.
        /// </summary>
        /// <param name="path">The <see cref="MemberExpression"/> you want to check.</param>
        /// <returns>True if the path defined was included in the object's graph.</returns>
        public bool Exist<TProperty>(Expression<Func<T, TProperty>> path) where TProperty : class
        {
            if (null == path)
                throw new ArgumentNullException("path");

            var stringPath = EvaluateExpression(path);

            if (!String.IsNullOrWhiteSpace(stringPath) && stringPath[0] == '.')
                stringPath = stringPath.Substring(1);

            return (from s in _includes
                    where s.StartsWith(stringPath, StringComparison.OrdinalIgnoreCase)
                    select s).Any();
        }


        /// <summary>
        /// If you have a member with deep graphing, this method gives you a new graph for the specified member. As the delegation of graph is allowed only by one level step, only the direct member selection is allowed.
        /// </summary>
        /// <typeparam name="TProperty">The member where wee need to extract the graph.</typeparam>
        /// <param name="path">The <see cref="MemberExpression"/> used to retrieve the name and the type used for the new Graph.</param>
        /// <returns>A new Graph instance with the type of the property selected in the path parameter.</returns>
        public Graph<TProperty> GetGraph<TProperty>(Expression<Func<T, TProperty>> path) where TProperty : class
        {
            if (null == path)
                throw new ArgumentNullException("path");

            var stringPath = EvaluateExpression(path);

            if (!String.IsNullOrWhiteSpace(stringPath) && stringPath[0] == '.')
                stringPath = stringPath.Substring(1);


            if (stringPath.Contains('.'))
                throw new AppException(OnlyLevelOneIsAllowed);

            var q = (from s in _includes
                     let paths = s.Split('.').ToList()
                     where paths[0] == stringPath && paths.Count > 1
                     select s.Substring(s.IndexOf('.') + 1)).ToList();

            return new Graph<TProperty>(q);
        }

        /// <summary>
        /// Parse an existing graph based on a object instance.
        /// </summary>
        /// <param name="instance">The object to parse.</param>
        /// <returns>The Graph for the dedicated object parsed.</returns>
        public static Graph<T> Parse(T instance, bool shouldHaveElement = false)
        {
            if (null == instance)
                throw new ArgumentNullException("instance");

            return shouldHaveElement ? new Graph<T>(ParseObject2(instance)) : new Graph<T>(ParseObject(instance));
        }

        /// <summary>
        /// Create a new Graph object where the paths starting with the path expression are removed.
        /// </summary>
        /// <typeparam name="TProperty">The property to remove</typeparam>
        /// <param name="path">The expression property to remove.</param>
        /// <returns>A copy of the graph with the removed path.</returns>
        public Graph<T> RemoveGraph<TProperty>(Expression<Func<T, TProperty>> path) where TProperty : class
        {
            if (null == path)
                throw new ArgumentNullException("path");

            var stringPath = EvaluateExpression(path);

            if (!String.IsNullOrWhiteSpace(stringPath) && stringPath[0] == '.')
                stringPath = stringPath.Substring(1);


            // remove from the Includes, all the includes containig the path.
            var l = (from i in Includes where !i.StartsWith(stringPath) select i).ToList();

            return new Graph<T>(l);
        }

        private static IEnumerable<string> ParseObject(object o)
        {
            var result = new List<String>();

            if (null == o)
                throw new ArgumentNullException("o");

            if (o is IEnumerable)
                throw new ArgumentException("Collection is not allowed", "instance");

            var type = o.GetType();

            if (!type.GetTypeInfo().IsClass)
                throw new ArgumentException("Only class type is allowed", "instance");

            if (type.IsArray)
                throw new ArgumentException("Array type is not allowed", "instance");

            var properties = type.GetRuntimeProperties();

            foreach (var property in properties)
            {

                object oProperty;
                if (property.PropertyType.IsArray)
                {
                    if (property.PropertyType.GetElementType() == typeof(String))
                        continue;

                    if (!property.PropertyType.GetElementType().GetTypeInfo().IsClass)
                        continue;

                    // Check if type instance is not null.
                    oProperty = property.GetValue(o, null);

                    var array = oProperty as Array;

                    if (null == array) continue;

                    if (array.Length == 0)
                    {
                        result.Add(property.Name);
                        continue;
                    }

                    var children = ParseObject(array.GetValue(0));
                    if (null == children || children.Count() == 0) result.Add(property.Name);
                    else
                        foreach (var child in children)
                        {
                            result.Add(String.Format("{0}.{1}", property.Name, child));
                        }

                    continue;
                }

                if (!property.PropertyType.GetTypeInfo().ImplementedInterfaces.All(t => t != typeof(IEnumerable)))
                {
                    if (!property.PropertyType.GetTypeInfo().IsGenericType)
                        continue;

                    oProperty = property.GetValue(o, null);
                    var enumerable = oProperty as IEnumerable;
                    if (null != enumerable)
                    {
                        var enumerator = enumerable.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            var children = ParseObject(enumerator.Current);
                            if (children.Count() == 0) result.Add(property.Name);
                            else
                                foreach (var child in children)
                                {
                                    result.Add(String.Format("{0}.{1}", property.Name, child));
                                }
                        }
                        else
                        {
                            result.Add(property.Name);
                        }
                    }
                    continue;
                }

                if (property.PropertyType == typeof(String))
                    continue;

                if (!property.PropertyType.GetTypeInfo().IsClass)
                    continue;


                // Check if type instance is not null.
                oProperty = property.GetValue(o, null);

                if (null != oProperty)
                {
                    var children = ParseObject(oProperty);
                    if (children.Count() == 0) result.Add(property.Name);
                    else
                        foreach (var child in children)
                        {
                            result.Add(String.Format("{0}.{1}", property.Name, child));
                        }

                }
                //var child = ParseObject()
            }


            return result;
        }

        private static IEnumerable<string> ParseObject2(object o)
        {
            var result = new List<String>();

            if (null == o)
                throw new ArgumentNullException("o");

            if (o is IEnumerable)
                throw new ArgumentException("Collection is not allowed", "instance");

            var type = o.GetType();

            if (!type.GetTypeInfo().IsClass)
                throw new ArgumentException("Only class type is allowed", "instance");

            if (type.IsArray)
                throw new ArgumentException("Array type is not allowed", "instance");

            var properties = type.GetRuntimeProperties();

            foreach (var property in properties)
            {

                object oProperty;
                if (property.PropertyType.IsArray)
                {
                    if (property.PropertyType.GetElementType() == typeof(String))
                        continue;

                    if (!property.PropertyType.GetElementType().GetTypeInfo().IsClass)
                        continue;

                    // Check if type instance is not null.
                    oProperty = property.GetValue(o, null);

                    var array = oProperty as Array;

                    if (null == array) continue;

                    if (array.Length == 0) // we graph here if an item is added to the list. If not we consider this is the same as null!
                    {
                        continue;
                    }

                    var children = ParseObject2(array.GetValue(0));
                    if (null == children || children.Count() == 0) result.Add(property.Name);
                    else
                        foreach (var child in children)
                        {
                            result.Add(String.Format("{0}.{1}", property.Name, child));
                        }

                    continue;
                }

                if (!property.PropertyType.GetTypeInfo().ImplementedInterfaces.All(t => t != typeof(IEnumerable)))
                {
                    if (!property.PropertyType.GetTypeInfo().IsGenericType)
                        continue;

                    oProperty = property.GetValue(o, null);
                    var enumerable = oProperty as IEnumerable;
                    if (null != enumerable)
                    {
                        var enumerator = enumerable.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            var children = ParseObject2(enumerator.Current);
                            if (children.Count() == 0) result.Add(property.Name);
                            else
                            if (children.Count() > 0)
                                foreach (var child in children)
                                {
                                    result.Add(String.Format("{0}.{1}", property.Name, child));
                                }
                        }
                    }
                    continue;
                }

                if (property.PropertyType == typeof(String))
                    continue;

                if (!property.PropertyType.GetTypeInfo().IsClass)
                    continue;


                // Check if type instance is not null.
                oProperty = property.GetValue(o, null);

                if (null != oProperty)
                {
                    var children = ParseObject2(oProperty);
                    if (children.Count() == 0) result.Add(property.Name);
                    else
                        foreach (var child in children)
                        {
                            result.Add(String.Format("{0}.{1}", property.Name, child));
                        }

                }
            }


            return result;
        }

        /// <summary>
        /// The read collection of includes defined.
        /// </summary>

        public IList<String> Includes
        {
            get
            {
                // Clone it before.
                return new List<String>(_includes);
            }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");

                ValidatePaths(value);

                _includes = new List<string>(value);
            }
        }


    }
}
