using Essentials.Utils.Extensions;

namespace Essentials.Database.EF.Attributes;

/// <summary>
/// Атрибут EF контекста
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EFContextAttribute : Attribute
{
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="metricTagValue">Название контекста в теге отдаваемой метрики</param>
    public EFContextAttribute(string metricTagValue)
    {
        MetricTagValue = metricTagValue.CheckNotNullOrEmpty(
            "Название контекста в теге отдаваемой метрики не может быть пустым");
    }

    /// <summary>
    /// Название контекста в теге отдаваемой метрики
    /// </summary>
    public string MetricTagValue { get; }
}