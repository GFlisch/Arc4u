using Arc4u.Configuration.Store.EFCore.Internals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.Configuration.Store;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbContextSectionStore<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.TryAddScoped<ISectionStore, DbContextSectionStore<TDbContext>>();
        return services;
    }
}

