namespace Essentials.Database.EF.Metrics.Options;

/// <summary>
/// Опции метрик
/// </summary>
public record MetricsOptions
{
    internal MetricsOptions(bool needMetrics)
    {
        NeedMetrics = needMetrics;
    }

    /// <summary>
    /// Признак необходимости отдавать метрики
    /// </summary>
    public bool NeedMetrics { get; }
}