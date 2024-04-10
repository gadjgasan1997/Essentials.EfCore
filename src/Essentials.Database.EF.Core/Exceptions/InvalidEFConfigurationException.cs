namespace Essentials.Database.EF.Exceptions;

/// <summary>
/// Исключение о неверной конфигурации базы данных
/// </summary>
public class InvalidEFConfigurationException : FormatException
{
    internal InvalidEFConfigurationException(string message)
        : base(
            "Во время конфигурации баз данных произошло исключение. Проверьте конфигурацию. " +
            $"{Environment.NewLine}Сообщение об ошибке: '{message}'")
    { }
}