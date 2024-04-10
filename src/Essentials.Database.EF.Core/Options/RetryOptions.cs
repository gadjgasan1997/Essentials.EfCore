using System.Diagnostics.CodeAnalysis;
using Essentials.Utils.Extensions;

namespace Essentials.Database.EF.Options;

/// <summary>
/// Опции повторения запросов
/// </summary>
public record RetryOptions
{
    internal RetryOptions(
        bool needRetry,
        int? retryCount = null,
        TimeSpan? retryDelay = null)
    {
        NeedRetry = needRetry;
        if (!NeedRetry) return;
        
        RetryCount = retryCount.CheckNotNull("В опциях ретраев необходимо указать количество повторений");
        RetryDelay = retryDelay.CheckNotNull("В опциях ретраев необходимо указать таймаут между повторениями");
    }

    /// <summary>
    /// Признак необходимости повторения
    /// </summary>
    [MemberNotNullWhen(true, nameof(RetryCount))]
    [MemberNotNullWhen(true, nameof(RetryDelay))]
    public bool NeedRetry { get; }
    
    /// <summary>
    /// Количество повторений
    /// </summary>
    public int? RetryCount { get; }

    /// <summary>
    /// Таймаут между повторениями
    /// </summary>
    public TimeSpan? RetryDelay { get; }
}