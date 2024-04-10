using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Oracle.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Infrastructure;
using Essentials.Database.EF.Options;
// ReSharper disable MemberCanBePrivate.Global

namespace Essentials.Database.EF.Extensions;

/// <summary>
/// Методы расширения для <see cref="DbContextOptionsBuilder" />
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Настраивает взаимодействие с базой Posgre без отслеживания изменений
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <param name="databaseOptions">Опции взаимодействия с базой данных</param>
    /// <param name="contextOptions">Опции контекста</param>
    /// <param name="npgsqlOptionsAction">Действие конфигурации взаимодействия с базой Postgre</param>
    public static void ConfigurePosgreDbNoTracking(
        this DbContextOptionsBuilder builder,
        DatabaseOptions databaseOptions,
        ContextOptions contextOptions,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        builder.ConfigurePosgreDb(
            databaseOptions,
            contextOptions,
            npgsqlOptionsAction,
            QueryTrackingBehavior.NoTracking);
    }
    
    /// <summary>
    /// Настраивает взаимодействие с базой Posgre
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <param name="databaseOptions">Опции взаимодействия с базой данных</param>
    /// <param name="contextOptions">Опции контекста</param>
    /// <param name="npgsqlOptionsAction">Действие конфигурации взаимодействия с базой Postgre</param>
    /// <param name="trackingBehavior">Вариант отслеживания изменений</param>
    public static void ConfigurePosgreDb(
        this DbContextOptionsBuilder builder,
        DatabaseOptions databaseOptions,
        ContextOptions contextOptions,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
        QueryTrackingBehavior? trackingBehavior = null)
    {
        builder.ConfigureContextProperties(contextOptions, trackingBehavior);
        
        if (!contextOptions.RetryOptions.NeedRetry)
        {
            builder.UseNpgsql(databaseOptions.ConnectionString, npgsqlOptionsAction);
            return;
        }
        
        builder.UseNpgsql(
            databaseOptions.ConnectionString,
            npgsqlOptionsAction: optionsBuilder =>
            {
                var retryOptions = contextOptions.RetryOptions;
                
                optionsBuilder.EnableRetryOnFailure(
                    retryOptions.RetryCount.Value,
                    retryOptions.RetryDelay.Value,
                    default);
                
                npgsqlOptionsAction?.Invoke(optionsBuilder);
            });
    }
    
    /// <summary>
    /// Настраивает взаимодействие с базой Oracle без отслеживания изменений
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <param name="databaseOptions">Опции взаимодействия с базой данных</param>
    /// <param name="contextOptions">Опции контекста</param>
    /// <param name="oracleOptionsAction">Действие конфигурации взаимодействия с базой Oracle</param>
    public static void ConfigureOracleDbNoTracking(
        this DbContextOptionsBuilder builder,
        DatabaseOptions databaseOptions,
        ContextOptions contextOptions,
        Action<OracleDbContextOptionsBuilder>? oracleOptionsAction = null)
    {
        builder.ConfigureOracleDb(
            databaseOptions,
            contextOptions,
            oracleOptionsAction,
            QueryTrackingBehavior.NoTracking);
    }

    /// <summary>
    /// Настраивает взаимодействие с базой Oracle
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <param name="databaseOptions">Опции взаимодействия с базой данных</param>
    /// <param name="contextOptions">Опции контекста</param>
    /// <param name="oracleOptionsAction">Действие конфигурации взаимодействия с базой Oracle</param>
    /// <param name="trackingBehavior">Вариант отслеживания изменений</param>
    public static void ConfigureOracleDb(
        this DbContextOptionsBuilder builder,
        DatabaseOptions databaseOptions,
        ContextOptions contextOptions,
        Action<OracleDbContextOptionsBuilder>? oracleOptionsAction = null,
        QueryTrackingBehavior? trackingBehavior = null)
    {
        builder.ConfigureContextProperties(contextOptions, trackingBehavior);
        
        if (!contextOptions.RetryOptions.NeedRetry)
        {
            builder.UseOracle(databaseOptions.ConnectionString, oracleOptionsAction);
            return;
        }
        
        builder.UseOracle(
            databaseOptions.ConnectionString,
            oracleOptionsAction: optionsBuilder =>
            {
                var retryOptions = contextOptions.RetryOptions;

                optionsBuilder.ExecutionStrategy(strategy =>
                    new OracleRetryingExecutionStrategy(
                        strategy.CurrentContext.Context,
                        retryOptions.RetryCount.Value,
                        retryOptions.RetryDelay.Value,
                        default));

                oracleOptionsAction?.Invoke(optionsBuilder);
            });
    }

    /// <summary>
    /// Настраивает общие свойства контекста
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <param name="contextOptions">Опции контекста</param>
    /// <param name="trackingBehavior">Вариант отслеживания изменений</param>
    private static void ConfigureContextProperties(
        this DbContextOptionsBuilder builder,
        ContextOptions contextOptions,
        QueryTrackingBehavior? trackingBehavior = null)
    {
        trackingBehavior ??= QueryTrackingBehavior.TrackAll;
        
        builder.UseQueryTrackingBehavior(trackingBehavior.Value);
        builder.EnableDetailedErrors(contextOptions.EnableDetailedErrors);
        builder.EnableSensitiveDataLogging(contextOptions.EnableSensitiveDataLogging);
    }
}