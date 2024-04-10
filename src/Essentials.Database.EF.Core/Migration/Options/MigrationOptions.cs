using System.Diagnostics.CodeAnalysis;
using Essentials.Utils.Extensions;

namespace Essentials.Database.EF.Migration.Options;

/// <summary>
/// Опции миграции
/// </summary>
public record MigrationOptions
{
    internal MigrationOptions(bool needMigrate, string? migrationName = null)
    {
        NeedMigrate = needMigrate;
        if (!needMigrate)
            return;
        
        MigrationName = migrationName.CheckNotNullOrEmpty(
            "В опциях применения миграций необходимо указать название миграции, " +
            $"если свойство '{nameof(NeedMigrate)}' проставлено в true");
    }

    /// <summary>
    /// Признак, надо ли выполнять миграции
    /// </summary>
    [MemberNotNullWhen(true, nameof(MigrationName))]
    public bool NeedMigrate { get; }

    /// <summary>
    /// Название миграции, до которой требуется произвести обновление
    /// </summary>
    public string? MigrationName { get; }
}