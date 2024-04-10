using Microsoft.Extensions.Configuration;
using Essentials.Database.EF.Metrics.Options;

namespace Essentials.Database.EF.Metrics.Extensions;

/// <summary>
/// Методы расширения для <see cref="IConfiguration"/>
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>
    /// Возвращает опции метрик
    /// </summary>
    /// <param name="section">Секция с опциями</param>
    /// <returns></returns>
    public static MetricsOptions GetMetricsOptions(this IConfigurationSection section)
    {
        var needRetry = section.GetValue<bool?>(nameof(MetricsOptions.NeedMetrics)) ?? false;
        return new MetricsOptions(needRetry);
    }
}