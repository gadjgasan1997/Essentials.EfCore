using Microsoft.EntityFrameworkCore;
using Essentials.Database.EF.Options;
using Essentials.Utils.Extensions;
using Microsoft.Extensions.Logging;
// ReSharper disable InvertIf

namespace Essentials.Database.EF.Migration.Implementations;

/// <inheritdoc cref="IEFMigrationService{TContext}" />
internal class EFMigrationService<TContext> : EFMigrationService, IEFMigrationService<TContext>
    where TContext : DbContext
{
    private readonly TContext _context;
    
    public EFMigrationService(
        ILoggerFactory loggerFactory,
        EFOptions options,
        TContext context,
        IServiceProvider serviceProvider)
        : base (loggerFactory, options, serviceProvider)
    {
        _context = context.CheckNotNull();
    }
    
    /// <inheritdoc cref="IEFMigrationService{TContext}.ApplyMigrationsAsync" />
    public async Task<ApplyMigrationState> ApplyMigrationsAsync()
    {
        var contextName = ContextName.Create(typeof(TContext));
        
        var contextOptions = Options.Databases.Values
            .SelectMany(options => options.Contexts)
            .FirstOrDefault(options => options.Name == contextName);
        
        if (contextOptions is null)
        {
            Logger.LogError(
                "Для применения миграции не найдены опции контекста с названием '{contextName}'",
                contextName);

            return ApplyMigrationState.Fail;
        }

        if (!contextOptions.MigrationOptions.NeedMigrate)
        {
            Logger.LogInformation("Для контекста с названием '{name}' отключены миграции", contextOptions.Name);
            return ApplyMigrationState.MigrationsDisable;
        }

        return await ApplyMigrationsAsync(_context, contextOptions);
    }
}