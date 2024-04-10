using Essentials.Utils.Extensions;

namespace Essentials.Database.EF.Options;

/// <summary>
/// Название контекста
/// </summary>
public readonly record struct ContextName
{
    private ContextName(string value)
    {
        Value = value
            .CheckNotNullOrEmpty("Название контекста не может быть пустым")
            .FullTrim()
            .ToLowerInvariant();
    }

    /// <summary>
    /// Название
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// Создает название контекста из строки
    /// </summary>
    /// <param name="value">Строка с названием контекста</param>
    /// <returns>Название контекста</returns>
    public static ContextName Create(string value) => new(value);
    
    /// <summary>
    /// Создает название контекста по типу
    /// </summary>
    /// <param name="type">Тип контекста</param>
    /// <returns>Название контекста</returns>
    public static ContextName Create(Type type) => new(type.FullName!);
}