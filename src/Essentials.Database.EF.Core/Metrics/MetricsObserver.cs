using App.Metrics;
using App.Metrics.Timer;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Essentials.Database.EF.Attributes;
using Essentials.Database.EF.Options;
using static Essentials.Database.EF.Metrics.MetricsRegistry;

namespace Essentials.Database.EF.Metrics;

/// <summary>
/// Наблюдатель для отдачи метрик
/// </summary>
internal class MetricsObserver : IObserver<KeyValuePair<string, object?>>
{
    private static readonly ConcurrentDictionary<Guid, Guid> _waitCounter = new();
    private static readonly ConcurrentDictionary<Guid, Guid> _activeCounter = new();
    private static readonly ConcurrentDictionary<Guid, Guid> _createdCounter = new();

    private readonly ConcurrentDictionary<Guid, TimerContext> _timers = new();

    private const string CONTEXT_NAME_TAG = "context_name";
    private static readonly MetricTags _unknownContextTags = new(CONTEXT_NAME_TAG, "unknown_context");
    
    private static readonly ConcurrentDictionary<DbContextId, ContextOptions?> _contextIdToOptionsMap = new();
    private static readonly ConcurrentDictionary<DbContextId, MetricTags> _contextIdToTagsMap = new();
    
    private readonly IMetrics _metrics;
    private readonly EFOptions _efOptions;

    public MetricsObserver(IMetrics metrics, EFOptions efOptions)
    {
        _metrics = metrics;
        _efOptions = efOptions;
    }
    
    public void OnCompleted() { }

    public void OnError(Exception error) { }

    public void OnNext(KeyValuePair<string, object?> value)
    {
        switch (value.Value)
        {
            case ConnectionCreatedEventData payload
                when NeedMetrics(payload.Context):
            {
                HandleConnectionCreatedMetricsEvent(value.Key, payload);
                break;
            }

            case ConnectionEventData payload
                when NeedMetrics(payload.Context):
            {
                HandleConnectionMetricsEvent(value.Key, payload);
                break;
            }

            case CommandExecutedEventData payload
                when NeedMetrics(payload.Context):
            {
                HandleCommandExecutedMetricsEvent(value.Key, payload);
                break;
            }

            case CommandErrorEventData payload
                when NeedMetrics(payload.Context):
            {
                HandleCommandErrorMetricsEvent(value.Key, payload);
                break;
            }
        }
    }

    private void HandleConnectionCreatedMetricsEvent(string key, ConnectionCreatedEventData payload)
    {
        if (key != RelationalEventId.ConnectionCreated.Name)
            return;

        IncrementCreatedCounter(payload.ConnectionId, payload.Context);
    }

    private void HandleConnectionMetricsEvent(string key, ConnectionEventData payload)
    {
        if (key == RelationalEventId.ConnectionDisposed.Name)
        {
            DecrementCreatedCounter(payload.ConnectionId, payload.Context);
            return;
        }

        if (key == RelationalEventId.ConnectionOpening.Name)
        {
            var timer = _metrics.Measure.Timer.Time(
                ConnectionTimer,
                GetContextTag(payload.Context));
            
            _timers.TryAdd(payload.ConnectionId, timer);
            
            IncrementWaitCounter(payload.ConnectionId, payload.Context);
            return;
        }

        if (key == RelationalEventId.ConnectionOpened.Name)
        {
            TryFinishConnectionTimer(payload.ConnectionId, payload.Context);
            IncrementOpenCounter(payload.ConnectionId, payload.Context);
            return;
        }

        if (key == RelationalEventId.ConnectionClosed.Name)
        {
            TryFinishConnectionTimer(payload.ConnectionId, payload.Context);
            DecrementOpenCounter(payload);
            return;
        }

        if (key == RelationalEventId.ConnectionError.Name)
        {
            _metrics.Measure.Counter.Increment(
                ConnectionErrorCounter,
                GetContextTag(payload.Context));
            
            TryFinishConnectionTimer(payload.ConnectionId, payload.Context);
        }
    }

    private void HandleCommandExecutedMetricsEvent(string key, CommandExecutedEventData payload)
    {
        if (key != RelationalEventId.CommandExecuted.Name)
            return;

        _metrics.Measure.Timer.Time(
            CommandExecutionTimer,
            GetContextTag(payload.Context),
            Convert.ToInt64(payload.Duration.TotalMilliseconds));

        TryFinishConnectionTimer(payload.ConnectionId, payload.Context);
    }

    private void HandleCommandErrorMetricsEvent(string key, CommandErrorEventData payload)
    {
        if (key != RelationalEventId.CommandError.Name)
            return;

        _metrics.Measure.Counter.Increment(
            CommandErrorCounter,
            GetContextTag(payload.Context));

        TryFinishConnectionTimer(payload.ConnectionId, payload.Context);
    }

    private void TryFinishConnectionTimer(Guid connectionId, DbContext? context)
    {
        if (!_timers.TryRemove(connectionId, out var timer))
            return;

        timer.Dispose();
        DecrementWaitCounter(connectionId, context);
    }

    private void IncrementCreatedCounter(Guid connectionId, DbContext? context)
    {
        if (!_createdCounter.TryAdd(connectionId, connectionId))
            return;

        _metrics.Measure.Gauge.SetValue(
            ConnectionCreatedCounter,
            GetContextTag(context),
            _createdCounter.Count);
    }

    private void DecrementCreatedCounter(Guid connectionId, DbContext? context)
    {
        if (!_createdCounter.TryRemove(connectionId, out _))
            return;

        _metrics.Measure.Gauge.SetValue(
            ConnectionCreatedCounter,
            GetContextTag(context),
            _createdCounter.Count);
    }

    private void IncrementWaitCounter(Guid connectionId, DbContext? context)
    {
        if (!_waitCounter.TryAdd(connectionId, connectionId))
            return;

        _metrics.Measure.Gauge.SetValue(
            ConnectionWaitingCounter,
            GetContextTag(context),
            _waitCounter.Count);
    }

    private void DecrementWaitCounter(Guid connectionId, DbContext? context)
    {
        if (!_waitCounter.TryRemove(connectionId, out _))
            return;

        _metrics.Measure.Gauge.SetValue(
            ConnectionWaitingCounter,
            GetContextTag(context),
            _waitCounter.Count);
    }

    private void IncrementOpenCounter(Guid connectionId, DbContext? context)
    {
        if (!_activeCounter.TryAdd(connectionId, connectionId))
            return;

        _metrics.Measure.Gauge.SetValue(
            ConnectionOpenCounter,
            GetContextTag(context),
            _activeCounter.Count);
    }

    private void DecrementOpenCounter(ConnectionEventData payload)
    {
        if (!_activeCounter.TryRemove(payload.ConnectionId, out _))
            return;

        _metrics.Measure.Gauge.SetValue(
            ConnectionOpenCounter,
            GetContextTag(payload.Context),
            _activeCounter.Count);
    }

    private bool NeedMetrics(DbContext? context)
    {
        if (context is null)
            return false;

        if (_contextIdToOptionsMap.TryGetValue(context.ContextId, out var contextOptions))
            return contextOptions?.MetricsOptions.NeedMetrics ?? false;

        var contextName = GetContextName(context);
        contextOptions = _contextIdToOptionsMap.GetOrAdd(
            context.ContextId,
            _ => _efOptions.Databases.Values
                .SelectMany(options => options.Contexts)
                .FirstOrDefault(options => options.Name == contextName));

        return contextOptions?.MetricsOptions.NeedMetrics ?? false;
    }

    private static MetricTags GetContextTag(DbContext? context)
    {
        if (context is null)
            return _unknownContextTags;
        
        return _contextIdToTagsMap.GetOrAdd(
            context.ContextId,
            _ =>
            {
                var name = GetContextMetricTagValue(context);
                return new MetricTags(CONTEXT_NAME_TAG, name);
            });
    }

    private static ContextName GetContextName(DbContext context) => ContextName.Create(context.GetType());

    private static string GetContextMetricTagValue(DbContext context)
    {
        var type = context.GetType();
        var attribute = type.GetCustomAttribute<EFContextAttribute>();
        return attribute?.MetricTagValue ?? type.FullName ?? "unknown_context";
    }
}