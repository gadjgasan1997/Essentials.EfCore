namespace Essentials.Database.EF.Options;

/// <summary>
/// Опции взаимодействия с EF
/// </summary>
public record EFOptions
{
    internal EFOptions()
    { }
    
    /// <summary>
    /// Мапа названий баз данных на их опции
    /// </summary>
    public Dictionary<string, DatabaseOptions> Databases { get; internal init; } = new();
    
    /// <summary>
    /// Мапа названий контекстов на базы данных, где они используются
    /// </summary>
    internal Dictionary<ContextName, DatabaseOptions> ContextToDatabasesMap { get; init; } = new();
    
    /// <summary>
    /// Мапа названий контекстов на их опции
    /// </summary>
    internal Dictionary<ContextName, ContextOptions> ContextToOptionsMap { get; init; } = new();
}