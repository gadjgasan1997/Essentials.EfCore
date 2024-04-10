using Microsoft.Extensions.Configuration;
using Essentials.Database.EF.Exceptions;
using Essentials.Database.EF.Metrics.Extensions;
using Essentials.Database.EF.Migration.Extensions;
using Essentials.Database.EF.Options;
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable LoopCanBeConvertedToQuery

namespace Essentials.Database.EF.Extensions;

/// <summary>
/// Методы расширения для <see cref="IConfiguration"/>
/// </summary>
internal static class ConfigurationExtensions
{
    private const string DATABASES_SECTION = "Databases";
    private const string CONTEXTS_SECTION = "Contexts";
    private const string RETRY_SECTION = "RetryOptions";
    private const string MIGRATION_SECTION = "MigrationOptions";
    private const string METRICS_SECTION = "MetricsOptions";
    
    /// <summary>
    /// Возвращает опции EF
    /// </summary>
    /// <param name="section">Секция с опциями</param>
    /// <returns></returns>
    /// <exception cref="InvalidEFConfigurationException"></exception>
    public static EFOptions GetEFOptions(this IConfigurationSection section)
    {
        var databasesSection = section.GetSection(DATABASES_SECTION);
        if (!databasesSection.Exists())
            throw new InvalidEFConfigurationException("В конфигурации EF отсутствует секция с опциями баз данных");

        var databases = databasesSection.GetDatabases();
        
        var contextToDatabasesMap = new Dictionary<ContextName, DatabaseOptions>();
        var contextToOptionsMap = new Dictionary<ContextName, ContextOptions>();

        foreach (var database in databases.Values)
        {
            foreach (var context in database.Contexts)
            {
                contextToDatabasesMap.Add(context.Name, database);
                contextToOptionsMap.Add(context.Name, context);
            }
        }
        
        return new EFOptions
        {
            Databases = databases,
            ContextToDatabasesMap = contextToDatabasesMap,
            ContextToOptionsMap = contextToOptionsMap
        };
    }

    /// <summary>
    /// Возвращает опции баз данных
    /// </summary>
    /// <param name="section">Секция с опциями</param>
    /// <returns></returns>
    private static Dictionary<string, DatabaseOptions> GetDatabases(this IConfigurationSection section)
    {
        var databases = new Dictionary<string, DatabaseOptions>();
        foreach (var databaseSection in section.GetChildren())
        {
            var database = databaseSection.GetDatabase();
            databases.Add(databaseSection.Key, database);
        }

        return databases;
    }

    /// <summary>
    /// Возвращает опции базы данных
    /// </summary>
    /// <param name="section">Секция с опциями</param>
    /// <returns></returns>
    private static DatabaseOptions GetDatabase(this IConfigurationSection section)
    {
        var connectionString = section.GetValue<string?>(nameof(DatabaseOptions.ConnectionString));
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidEFConfigurationException(
                $"Не указана строка подключения к базе данных '{section.Key}'");
        }
            
        var contextsSection = section.GetSection(CONTEXTS_SECTION);
        if (!contextsSection.Exists())
            throw new InvalidEFConfigurationException("В конфигурации БД отсутствует секция с контекстами EF");

        var contexts = contextsSection.GetContexts().ToList();
        return new DatabaseOptions(connectionString, contexts);
    }

    /// <summary>
    /// Возвращает опции контекстов
    /// </summary>
    /// <param name="section">Секция с опциями</param>
    /// <returns></returns>
    private static IEnumerable<ContextOptions> GetContexts(this IConfigurationSection section)
    {
        foreach (var contextSection in section.GetChildren())
            yield return contextSection.GetContext();
    }

    /// <summary>
    /// Возвращает опции контекста
    /// </summary>
    /// <param name="section">Секция с опциями</param>
    /// <returns></returns>
    private static ContextOptions GetContext(this IConfigurationSection section)
    {
        var name = section.GetValue<string>(nameof(ContextOptions.Name));
        var contextName = ContextName.Create(name!);

        var poolSize = section.GetValue<int?>(nameof(ContextOptions.PoolSize));
        var enableDetailedErrors = section.GetValue<bool?>(nameof(ContextOptions.EnableDetailedErrors));
        var enableSensitiveDataLogging = section.GetValue<bool?>(nameof(ContextOptions.EnableSensitiveDataLogging));
        
        var retryOptions = section.GetSection(RETRY_SECTION).GetRetryOptions();
        var migrationOptions = section.GetSection(MIGRATION_SECTION).GetMigrationOptions();
        var metricsOptions = section.GetSection(METRICS_SECTION).GetMetricsOptions();

        return new ContextOptions(
            contextName,
            poolSize,
            enableDetailedErrors,
            enableSensitiveDataLogging,
            retryOptions,
            migrationOptions,
            metricsOptions);
    }

    /// <summary>
    /// Возвращает опции ретраев
    /// </summary>
    /// <param name="section">Секция с опциями</param>
    /// <returns></returns>
    private static RetryOptions GetRetryOptions(this IConfigurationSection section)
    {
        var needRetry = section.GetValue<bool?>(nameof(RetryOptions.NeedRetry)) ?? false;
        var retryCount = section.GetValue<int?>(nameof(RetryOptions.RetryCount));
        var retryDelay = section.GetValue<TimeSpan?>(nameof(RetryOptions.RetryDelay));

        return new RetryOptions(needRetry, retryCount, retryDelay);
    }
}