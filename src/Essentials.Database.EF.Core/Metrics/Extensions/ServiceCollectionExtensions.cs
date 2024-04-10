using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Essentials.Database.EF.Metrics.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает метрики для EF Core
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureEFMetrics(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddHostedService<MetricsHostedService>();
    }
}