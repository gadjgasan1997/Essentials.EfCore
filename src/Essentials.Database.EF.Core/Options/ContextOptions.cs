using Essentials.Database.EF.Metrics.Options;
using Essentials.Database.EF.Migration.Options;

namespace Essentials.Database.EF.Options;

/// <summary>
/// Опции контекста
/// </summary>
public record ContextOptions
{
    internal ContextOptions(
        ContextName name,
        int? poolSize,
        bool? usePool,
        bool? enableDetailedErrors,
        bool? enableSensitiveDataLogging,
        RetryOptions? retryOptions,
        MigrationOptions? migrationOptions,
        MetricsOptions? metricsOptions)
    {
        Name = name;
        UsePool = usePool ?? false;
        PoolSize = poolSize ?? 1024;
        EnableDetailedErrors = enableDetailedErrors ?? false;
        EnableSensitiveDataLogging = enableSensitiveDataLogging ?? false;
        RetryOptions = retryOptions ?? new RetryOptions(false);
        MigrationOptions = migrationOptions ?? new MigrationOptions(false);
        MetricsOptions = metricsOptions ?? new MetricsOptions(false);
    }

    /// <summary>
    /// Название
    /// </summary>
    public ContextName Name { get; }
    
    /// <summary>
    /// Признак необходимости использовать пул
    /// </summary>
    public bool UsePool { get; }
    
    /// <summary>
    /// Размера пула
    /// </summary>
    public int PoolSize { get; }
    
    /// <summary>
    /// Признак необходимости включать подробную информацию об ошибках в логи
    /// </summary>
    public bool EnableDetailedErrors { get; }
    
    /// <summary>
    /// Признак необходимости включать чувствительную в логи
    /// </summary>
    public bool EnableSensitiveDataLogging { get; }

    /// <summary>
    /// Опции повторения запросов
    /// </summary>
    public RetryOptions RetryOptions { get; }

    /// <summary>
    /// Опции миграции
    /// </summary>
    public MigrationOptions MigrationOptions { get; }

    /// <summary>
    /// Опции метрик
    /// </summary>
    public MetricsOptions MetricsOptions { get; }
}