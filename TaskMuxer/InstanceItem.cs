using System.Diagnostics.CodeAnalysis;

namespace TaskMuxer;

[ExcludeFromCodeCoverage]
internal record InstanceItem
{
    internal required ItemKey Key { get; init; }
    internal virtual object Value { get; init; } = default!;
    internal virtual object Func { get; init; } = default!;
    internal required CancellationTokenSource InternalCancellationSource { get; init; }
    internal required CancellationTokenSource LinkedCancellationTokenSource { get; init; }
    internal required ItemStatus Status { get; set; }
}

[ExcludeFromCodeCoverage]
internal record InstanceItem<T> : InstanceItem
{
    private TaskCompletionSource<T?> _value = default!;
    private Func<CancellationToken, Task<T?>> _func = default!;

    internal required new TaskCompletionSource<T?> Value
    {
        get => _value;
        init => base.Value = _value = value;
    }

    internal required new Func<CancellationToken, Task<T?>> Func
    {
        get => _func;
        init => base.Func = _func = value;
    }
}
