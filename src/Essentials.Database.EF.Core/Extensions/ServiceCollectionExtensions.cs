using Essentials.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Essentials.Database.EF.Exceptions;
using Essentials.Database.EF.Metrics.Extensions;
using Essentials.Database.EF.Migration.Extensions;
using Essentials.Database.EF.Options;
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable MemberCanBePrivate.Global

namespace Essentials.Database.EF.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string EF_SECTION = "EFOptions";

    private static uint _isConfigured;
    
    /// <summary>
    /// Настраивает контекст EF
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureAction">Действие по конфигурации</param>
    /// <typeparam name="TContext">Тип контекста</typeparam>
    /// <returns></returns>
    public static IServiceCollection ConfigureEFContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DatabaseOptions, ContextOptions> configureAction)
        where TContext : DbContext
    {
        return services
            .ConfigureEFOptions(configuration)
            .ConfigureEFMigrationService<TContext>()
            .ConfigureWithRegisteredService<EFOptions>(options =>
            {
                var contextName = ContextName.Create(typeof(TContext));
                
                if (!options.ContextToDatabasesMap.TryGetValue(contextName, out var databaseOptions) ||
                    !options.ContextToOptionsMap.TryGetValue(contextName, out var contextOptions))
                {
                    throw new InvalidEFConfigurationException(
                        $"Для контекста '{contextName}' не настроена конфигурация");
                }

                configureAction(databaseOptions, contextOptions);
            });
    }
    
    /// <summary>
    /// Настраивает опции взаимодействия с EF
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidEFConfigurationException"></exception>
    public static IServiceCollection ConfigureEFOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AtomicConfigureService(
            ref _isConfigured,
            () => services.ConfigureEFOptionsPrivate(configuration));
    }

    /// <summary>
    /// Настраивает опции взаимодействия с EF
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidEFConfigurationException"></exception>
    private static void ConfigureEFOptionsPrivate(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(EF_SECTION);
        if (!section.Exists())
            throw new InvalidEFConfigurationException("В конфигурации отсутствует секция с опциями EF");

        var options = section.GetEFOptions();

        services.TryAddSingleton(options);

        services
            .ConfigureEFMetrics(configuration)
            .ConfigureEFMigrationService();
    }
}