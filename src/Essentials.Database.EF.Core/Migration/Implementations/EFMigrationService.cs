using Essentials.Database.EF.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Essentials.Database.EF.Options;
using Essentials.Utils.Extensions;
using Essentials.Utils.Reflection.Extensions;
using Essentials.Utils.Reflection.Helpers;
using Microsoft.Extensions.Logging;
// ReSharper disable InvertIf
// ReSharper disable MemberCanBeProtected.Global

namespace Essentials.Database.EF.Migration.Implementations;

/// <inheritdoc cref="IEFMigrationService" />
internal class EFMigrationService : IEFMigrationService
{
    private readonly IServiceProvider _serviceProvider;
    
    protected ILogger Logger { get; }
    protected EFOptions Options { get; }
    
    public EFMigrationService(
        ILoggerFactory loggerFactory,
        EFOptions options,
        IServiceProvider serviceProvider)
    {
        Logger = loggerFactory.CreateLogger(LoggersNames.MigrationsLogger);
        _serviceProvider = serviceProvider.CheckNotNull();
        Options = options.CheckNotNull();
    }
    
    /// <inheritdoc cref="IEFMigrationService.ApplyMigrationsAsync" />
    public async Task<ApplyMigrationState> ApplyMigrationsAsync(string name, DatabaseOptions options)
    {
        try
        {
            var assemblies = AssemblyHelpers.GetCurrentDomainAssemblies().ToArray();

            foreach (var contextOptions in options.Contexts)
            {
                if (!contextOptions.MigrationOptions.NeedMigrate)
                {
                    Logger.LogInformation(
                        "Для контекста с названием '{name}' отключены миграции",
                        contextOptions.Name);
                    
                    continue;
                }

                var type = assemblies.GetTypeByName(contextOptions.Name.Value, StringComparison.InvariantCultureIgnoreCase);
                
                var context = (DbContext) _serviceProvider.GetRequiredService(type);

                var state = await ApplyMigrationsAsync(context, contextOptions);
                if (state is ApplyMigrationState.Fail)
                    return state;
            }

            return ApplyMigrationState.Success;
        }
        catch (Exception exception)
        {
            Logger.LogError(
                exception,
                "Во время применения миграций для контекстов базы данных произошло исключение. " +
                "Название БД: '{name}'",
                name);
            
            return ApplyMigrationState.Fail;
        }
    }

    /// <summary>
    /// Применяет миграции к контексту базы данных
    /// </summary>
    /// <param name="context">Контекст</param>
    /// <param name="contextOptions">Опции контекста</param>
    /// <returns></returns>
    protected async Task<ApplyMigrationState> ApplyMigrationsAsync(DbContext context, ContextOptions contextOptions)
    {
        var migrationName = contextOptions.MigrationOptions.MigrationName;
        
        try
        {
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            if (appliedMigrations.LastOrDefault() == migrationName)
            {
                Logger.LogWarning("База уже обновлена до миграции '{migrationName}'.", migrationName);
                return ApplyMigrationState.AlreadyApplied;
            }

            var migrator = context.Database.GetInfrastructure().GetRequiredService<IMigrator>();
            await migrator.MigrateAsync(migrationName);
        }
        catch (Exception exception)
        {
            Logger.LogError(
                exception,
                "Во время обновления базы до выбранной миграции произошло исключение. " +
                "Название миграции: '{migrationName}'. Название контекста: '{name}'.",
                migrationName, contextOptions.Name);

            return ApplyMigrationState.Fail;
        }
        
        Logger.LogInformation("База была успешно обновлена до миграции '{migrationName}'.", migrationName);
        return ApplyMigrationState.Success;
    }
}