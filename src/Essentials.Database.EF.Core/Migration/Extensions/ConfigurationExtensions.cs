using Microsoft.Extensions.Configuration;
using Essentials.Database.EF.Migration.Options;

namespace Essentials.Database.EF.Migration.Extensions;

/// <summary>
/// Методы расширения для <see cref="IConfiguration"/>
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>
    /// Возвращает опции применения миграций
    /// </summary>
    /// <param name="section">Секция с опциями</param>
    /// <returns></returns>
    public static MigrationOptions GetMigrationOptions(this IConfigurationSection section)
    {
        var needMigrate = section.GetValue<bool?>(nameof(MigrationOptions.NeedMigrate)) ?? false;
        var migrationName = section.GetValue<string?>(nameof(MigrationOptions.MigrationName));

        return new MigrationOptions(needMigrate, migrationName);
    }
}