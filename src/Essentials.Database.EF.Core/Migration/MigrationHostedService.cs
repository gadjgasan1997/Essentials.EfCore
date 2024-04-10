using Essentials.Database.EF.Dictionaries;
using Microsoft.Extensions.Hosting;
using Essentials.Database.EF.Options;
using Essentials.Utils.Extensions;
using Microsoft.Extensions.Logging;

namespace Essentials.Database.EF.Migration;

/// <summary>
/// Сервис для автоматического применения миграций к БД
/// </summary>
internal class MigrationHostedService : IHostedService
{
    private readonly EFOptions _options;
    private readonly IEFMigrationService _efMigrationService;
    private readonly IHostApplicationLifetime _lifeTime;
    private readonly ILogger _logger;
    
    public MigrationHostedService(
        ILoggerFactory loggerFactory,
        EFOptions options,
        IEFMigrationService efMigrationService,
        IHostApplicationLifetime lifetime)
    {
        _logger = loggerFactory.CreateLogger(LoggersNames.MigrationsLogger);
        _options = options.CheckNotNull();
        _efMigrationService = efMigrationService.CheckNotNull();
        _lifeTime = lifetime.CheckNotNull();
    }
    
    /// <inheritdoc cref="IHostedService.StartAsync" />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var (name, options) in _options.Databases)
            await ApplyMigrationsAsync(name, options);
    }

    /// <inheritdoc cref="IHostedService.StopAsync" />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Применяет миграции к базе данных
    /// </summary>
    /// <param name="name">Название базы данных</param>
    /// <param name="options">Опции базы данных</param>
    private async Task ApplyMigrationsAsync(string name, DatabaseOptions options)
    {
        var state = await _efMigrationService.ApplyMigrationsAsync(name, options);
        if (state is not ApplyMigrationState.Fail)
            return;
        
        _logger.LogError(
            "Во время автоматического применения миграций к базе данных с названием '{name}' " +
            "произошла ошибка. Приложение будет остановлено",
            name);
        
        _lifeTime.StopApplication();
    }
}