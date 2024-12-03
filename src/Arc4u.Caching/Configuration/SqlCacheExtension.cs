using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Configuration.Sql;
public static class SqlCacheExtension
{
    public static IServiceCollection AddSqlCache(this IServiceCollection services, [DisallowNull] string name, Action<SqlCacheOption> options)
    {
        var validate = new SqlCacheOption();
        new Action<SqlCacheOption>(options).Invoke(validate);

        ArgumentException.ThrowIfNullOrEmpty(name);

        services.Configure<SqlCacheOption>(name, options);

        return services;
    }

    public static IServiceCollection AddSqlCache(this IServiceCollection services, [DisallowNull] string name, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName)
    {
        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<SqlCacheOption>();

            if (option is null)
            {
                throw new NullReferenceException(nameof(option));
            }

            void options(SqlCacheOption o)
            {
                o.SerializerName = option.SerializerName;
                o.SchemaName = option.SchemaName;
                o.TableName = option.TableName;
                o.ConnectionString = option.ConnectionString;
            }

            services.AddSqlCache(name, options);
        }

        return services;
    }

}
