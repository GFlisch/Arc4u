using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Arc4u.EfCore;

public static class GraphExtension
{
    private const string OnlyLevelOneIsAllowed = "It is not allowed to check more than one level!";

    /// <summary>
    /// Will apply the includes on the <see cref="IQueryable&gt;T&lt;"/> but the types having an assoication based on <see cref="IEnumerable"/>.
    /// </summary>
    /// <param name="query">A <see cref="IQueryable&gt;T&lt;"/> object where the Entity Framework adds an extension Include method.</param>
    /// <returns>The <see cref="IQueryable&gt;T&lt;"/> object with all the includes applied but the types having an assoication based on <see cref="IEnumerable"/>.</returns>
    public static IQueryable<T> ApplySingleReferences<T>(this Graph<T> graph, IQueryable<T> query) where T : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var dQuery = query; // will invoke the Include method.
        var objectType = typeof(T);
        // will check that the field is of type class and not a IEnumerable type.

        foreach (var i in graph.Includes)
        {
            // Check for single field if it is an association list or not!
            var path = i.Split('.');
            if (path.Length == 1)
            {
                if (objectType.GetRuntimeProperty(i)!
                              .PropertyType.GetTypeInfo()
                              .ImplementedInterfaces.All(t => t != typeof(IEnumerable)))
                {
                    dQuery = dQuery.Include(i);
                }
            }
            else
            {
                var type = objectType;
                // var bSimpleGraph = true;
                var includedItems = new List<string>();
                foreach (var propertyName in path)
                {
                    type = type.GetRuntimeProperty(propertyName)!.PropertyType;
                    if (type.GetTypeInfo().ImplementedInterfaces.All(t => t != typeof(IEnumerable)))
                    {
                        includedItems.Add(propertyName);
                        continue;
                    }

                    // bSimpleGraph = false;
                    break;
                }

                // if (bSimpleGraph)
                if (includedItems.Count > 0)
                {
                    dQuery = dQuery.Include(string.Join(".", includedItems));
                }
            }

        }

        return dQuery;
    }

    /// <summary>
    /// Will apply the includes on the <see cref="IQueryable&gt;T&lt;"/> without exceptions.
    /// </summary>
    /// <param name="query">A <see cref="IQueryable&gt;T&lt;"/> object where the Entity Framework adds an extension Include method.</param>
    /// <returns>The <see cref="IQueryable&gt;T&lt;"/> object with all the includes applied.</returns>
    public static IQueryable<T> ApplySetReferences<T>(this Graph<T> graph, IQueryable<T> query) where T : class
    {
        return graph.Includes.Aggregate<string, IQueryable<T>>(query, (queryable, i) => queryable.BuildInclude(i) ?? queryable);
    }

    public static IQueryable<T> ApplyReferences<TProperty, T>(this Graph<T> graph, IQueryable<T> query, Expression<Func<T, TProperty>> path) where TProperty : class where T : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var stringPath = Graph<T>.EvaluateExpression(path);

        if (stringPath.Contains('.'))
        {
            throw new AppException(OnlyLevelOneIsAllowed);
        }

        return graph.Includes.Where(i => i.StartsWith(stringPath)).Aggregate<string, IQueryable<T>>(query, (queryable, i) => queryable.BuildInclude(i) ?? queryable);
    }

    /// <summary>
    /// Perform an Include<T> on the first property path and after a ThenInclude!
    /// </summary>
    /// <typeparam name="T">The aggregate root object.</typeparam>
    /// <param name="queryable">Queryable object to include(s).</param>
    /// <param name="propertyPath">A dot path structure to build the Include and ThenIncludes.</param>
    /// <returns></returns>
    private static IQueryable<T>? BuildInclude<T>(this IQueryable<T> queryable, string propertyPath) where T : class
    {
        var properties = propertyPath.Split(new[] { '.' });

        var thenIncludeObject = BuildInternalInclude(queryable, properties[0], out var propertyType);

        for (var i = 1; i < properties.Length; i++)
        {
            thenIncludeObject = BuildThenInclude(thenIncludeObject, typeof(T), propertyType, properties[i], out var subProperty);
            propertyType = subProperty;
        }

        return thenIncludeObject as IQueryable<T>;

    }

    private static object BuildInternalInclude<T>(IQueryable<T> queryable, string property, out Type propertyType) where T : class
    {
        propertyType = typeof(T).GetProperty(property)!.PropertyType;

        var pe = Expression.Parameter(typeof(T), "p");
        var expr = Expression.Lambda(Expression.Property(pe, property), pe);

        var includeMethodInfo = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(mi => mi.Name == "Include"
                    && mi.IsGenericMethodDefinition
                    && mi.GetGenericArguments().Length == 2
                    && mi.GetParameters().Length == 2);

        var result = includeMethodInfo.MakeGenericMethod(new[] { typeof(T), propertyType }).Invoke(queryable, new object[] { queryable, expr });
        if (result == null)
        {
            throw new InvalidOperationException("Include method invocation returned null.");
        }
        return result;
    }

    private static object BuildThenInclude(object queryable, Type parentType, Type propertyType, string subProperty, out Type subPropertyType)
    {
        var basePropertyType = propertyType.IsGenericType ? propertyType.GetGenericArguments().First() : propertyType;
        subPropertyType = basePropertyType.GetProperty(subProperty)!.PropertyType;

        ParameterExpression pe = Expression.Parameter(basePropertyType, "p");
        var expr = Expression.Lambda(Expression.Property(pe, subProperty), pe);

        var l = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(mi => mi.Name == "ThenInclude"
                && mi.IsGenericMethodDefinition
                && mi.GetGenericArguments().Length == 3
                && mi.GetParameters().Length == 2).ToList();

        var generic = l.Where(p0 => p0.GetParameters().Any(p1 => p1.ParameterType.GetGenericArguments().Any(g => g.Name.Equals("IEnumerable`1")))).First();

        var m = propertyType.IsGenericType ? generic : l.Where(t => t != generic).First();

        if (m == null)
        {
            throw new InvalidOperationException("ThenInclude method not found.");
        }

        return m.MakeGenericMethod(new[] { parentType, basePropertyType, subPropertyType }).Invoke(queryable, new object[] { queryable, expr })!;
    }
}
