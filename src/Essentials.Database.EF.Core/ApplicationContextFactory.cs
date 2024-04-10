using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Essentials.Utils.Extensions;
// ReSharper disable StaticMemberInGenericType

namespace Essentials.Database.EF;

/// <summary>
/// Фабрика для создания контекста
/// </summary>
/// <typeparam name="TContext">Тип контекста</typeparam>
public abstract class ApplicationContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    private const string MIGRATIONS_CONNECTION_STRING_ENV = "MigrationsConnectionString";
    
    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(_ => { });

    /// <inheritdoc cref="IDesignTimeDbContextFactory{TContext}" />
    public abstract TContext CreateDbContext(string[] args);

    /// <summary>
    /// Создает контекст
    /// </summary>
    /// <param name="func">Делегат создания контекста из опций</param>
    /// <param name="connectionString">Строка подключения к БД</param>
    /// <returns></returns>
    protected TContext CreateDbContext(
        Func<DbContextOptionsBuilder<TContext>, TContext> func,
        string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment
                .GetEnvironmentVariable(MIGRATIONS_CONNECTION_STRING_ENV)
                .CheckNotNullOrEmpty(
                    $"Не указана строка подключения для применения миграции к контексту '{typeof(TContext).FullName}'. " +
                    $"Для применения миграции проставьте переменную среды '{MIGRATIONS_CONNECTION_STRING_ENV}'.",
                    MIGRATIONS_CONNECTION_STRING_ENV);
        }
        
        var optionsBuilder = new DbContextOptionsBuilder<TContext>()
            .UseLoggerFactory(_loggerFactory)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .UseNpgsql(connectionString);

        return func(optionsBuilder);
    }
}