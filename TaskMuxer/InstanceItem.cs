using System.Diagnostics.CodeAnalysis;

namespace TaskMuxer;

[ExcludeFromCodeCoverage]
internal record InstanceItem
{
    internal virtual object TaskCompletionSource { get; init; } = default!;
    internal required CancellationTokenSource InternalCancellationTokenSource { get; init; }
    internal required CancellationTokenSource PreserveExecutionResultCancellationTokenSource { get; init; }
    internal required ItemStatus Status { get; set; }
}

[ExcludeFromCodeCoverage]
internal record InstanceItem<T> : InstanceItem
{
    private TaskCompletionSource<T?> _value = default!;

    internal required new TaskCompletionSource<T?> TaskCompletionSource
    {
        get => _value;
        init => base.TaskCompletionSource = _value = value;
    }
}
