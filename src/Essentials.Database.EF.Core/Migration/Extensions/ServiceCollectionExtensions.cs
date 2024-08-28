using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Essentials.Database.EF.Migration.Implementations;

namespace Essentials.Database.EF.Migration.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает сервис управления миграциями для EF
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TContext">Тип контекста</typeparam>
    /// <returns></returns>
    public static IServiceCollection ConfigureEFMigrationService<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services
            .ConfigureEFMigrationService()
            .TryAddScoped<IEFMigrationService<TContext>, EFMigrationService<TContext>>();
        
        return services;
    }
    
    /// <summary>
    /// Настраивает сервис управления миграциями для EF
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection ConfigureEFMigrationService(this IServiceCollection services)
    {
        services.AddHostedService<MigrationHostedService>();
        services.TryAddScoped<IEFMigrationService, EFMigrationService>();
        
        return services;
    }
}