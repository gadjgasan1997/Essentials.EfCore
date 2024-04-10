using System.Diagnostics;
using App.Metrics;
using Microsoft.EntityFrameworkCore;
using Essentials.Database.EF.Options;

namespace Essentials.Database.EF.Metrics;

/// <summary>
/// Обработчик для отдачи метрик
/// </summary>
internal class MetricsDiagnosticsHandler : IObserver<DiagnosticListener>
{
    private readonly IMetrics _metrics;
    private readonly EFOptions _options;
    private readonly List<IDisposable> _subscriptions = new();
    
    public MetricsDiagnosticsHandler(IMetrics metrics, EFOptions options)
    {
        _metrics = metrics;
        _options = options;
    }

    public void OnNext(DiagnosticListener listener)
    {
        if (listener.Name != DbLoggerCategory.Name)
            return;
        
        var subscription = listener.Subscribe(new MetricsObserver(_metrics, _options));
        _subscriptions.Add(subscription);
    }

    public void OnError(Exception error) { }

    public void OnCompleted()
    {
        _subscriptions.ForEach(x => x.Dispose());
        _subscriptions.Clear();
    }
}