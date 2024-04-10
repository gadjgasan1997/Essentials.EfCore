using Essentials.Utils.Extensions;

namespace Essentials.Database.EF.Options;

/// <summary>
/// Опции взаимодействия с базой данных
/// </summary>
public record DatabaseOptions
{
    internal DatabaseOptions(string connectionString, List<ContextOptions> contexts)
    {
        ConnectionString = connectionString.CheckNotNullOrEmpty();
        Contexts = contexts;
    }

    /// <summary>
    /// Строка подключения
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Контексты
    /// </summary>
    public List<ContextOptions> Contexts { get; }
}