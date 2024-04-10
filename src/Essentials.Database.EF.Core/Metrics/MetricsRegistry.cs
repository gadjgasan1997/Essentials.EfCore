using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Timer;

namespace Essentials.Database.EF.Metrics;

/// <summary>
/// Реестр метрик
/// </summary>
internal static class MetricsRegistry
{
    /// <summary>
    /// Счетчик количества соединений, ожидающих открытия
    /// </summary>
    public static readonly GaugeOptions ConnectionWaitingCounter = new()
    {
        Name = "DbContext.Connection.Waiting",
        MeasurementUnit = Unit.Connections
    };

    /// <summary>
    /// Счетчик количества открытых соединений
    /// </summary>
    public static readonly GaugeOptions ConnectionOpenCounter = new()
    {
        Name = "DbContext.Connection.Open",
        MeasurementUnit = Unit.Connections
    };

    /// <summary>
    /// Счетчик количества созданных соединений
    /// </summary>
    public static readonly GaugeOptions ConnectionCreatedCounter = new()
    {
        Name = "DbContext.Connection.Created",
        MeasurementUnit = Unit.Connections
    };

    /// <summary>
    /// Счетчик количества открытых соединений
    /// </summary>
    public static readonly CounterOptions ConnectionErrorCounter = new()
    {
        Name = "DbContext.Connection.Error",
        MeasurementUnit = Unit.Connections
    };

    /// <summary>
    /// Таймер времени подключения
    /// </summary>
    public static readonly TimerOptions ConnectionTimer = new()
    {
        Name = "DbContext.Connection.ConnectionTimer",
        MeasurementUnit = Unit.Connections,
        DurationUnit = TimeUnit.Milliseconds,
        RateUnit = TimeUnit.Milliseconds
    };

    /// <summary>
    /// Счетчик количества ошибочно выполненных запросов
    /// </summary>
    public static readonly CounterOptions CommandErrorCounter = new()
    {
        Name = "DbContext.Command.Error",
        MeasurementUnit = Unit.Requests
    };

    /// <summary>
    /// Таймер времени выполнения запроса
    /// </summary>
    public static readonly TimerOptions CommandExecutionTimer = new()
    {
        Name = "DbContext.Command.Execution",
        MeasurementUnit = Unit.Requests,
        DurationUnit = TimeUnit.Milliseconds,
        RateUnit = TimeUnit.Milliseconds
    };
}