using System.Diagnostics;
using App.Metrics;
using Microsoft.Extensions.Hosting;
using Essentials.Database.EF.Options;

namespace Essentials.Database.EF.Metrics;

/// <summary>
/// Сервис для отдачи метрик
/// </summary>
internal class MetricsHostedService : IHostedService
{
    private readonly EFOptions _options;
    private readonly IMetrics _metrics;

    public MetricsHostedService(EFOptions options, IMetrics metrics)
    {
        _metrics = metrics;
        _options = options;
    }

    /// <inheritdoc cref="IHostedService.StartAsync" />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var contexts = _options.Databases.Values.SelectMany(options => options.Contexts).ToList();
        if (contexts.All(options => !options.MetricsOptions.NeedMetrics))
            return Task.CompletedTask;
        
        var diagnosticsHandler = new MetricsDiagnosticsHandler(_metrics, _options);
        DiagnosticListener.AllListeners.Subscribe(diagnosticsHandler);
        
        return Task.CompletedTask;
    }
    
    /// <inheritdoc cref="IHostedService.StopAsync" />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}