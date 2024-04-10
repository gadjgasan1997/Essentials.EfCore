namespace Essentials.Database.EF.Migration;

/// <summary>
/// Статус применения миграции
/// </summary>
public enum ApplyMigrationState
{
    /// <summary>
    /// Ошибка применения миграции
    /// </summary>
    Fail = 0,
    
    /// <summary>
    /// Миграции отключены
    /// </summary>
    MigrationsDisable = 1,
    
    /// <summary>
    /// База уже была обновлена до выбранной миграции
    /// </summary>
    AlreadyApplied = 2,
    
    /// <summary>
    /// Успех применения миграции
    /// </summary>
    Success = 3
}