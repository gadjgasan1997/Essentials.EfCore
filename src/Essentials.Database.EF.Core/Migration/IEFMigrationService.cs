using Microsoft.EntityFrameworkCore;
using Essentials.Database.EF.Options;

namespace Essentials.Database.EF.Migration;

/// <summary>
/// Сервис управления миграциями для EF
/// </summary>
public interface IEFMigrationService
{
    /// <summary>
    /// Применяет миграции к базе данных
    /// </summary>
    /// <param name="name">Название базы данных</param>
    /// <param name="options">Опции взаимодействия с базой данных</param>
    /// <returns></returns>
    Task<ApplyMigrationState> ApplyMigrationsAsync(string name, DatabaseOptions options);
}

/// <summary>
/// Сервис управления миграциями для EF
/// <typeparam name="TContext">Тип контекста</typeparam>
/// </summary>
public interface IEFMigrationService<TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Применяет миграции к базе данных
    /// </summary>
    /// <returns></returns>
    Task<ApplyMigrationState> ApplyMigrationsAsync();
}