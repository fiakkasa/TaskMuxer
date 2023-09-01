using System.Diagnostics.CodeAnalysis;

namespace TaskMuxer;

[ExcludeFromCodeCoverage]
internal record InstanceItem(object Value, CancellationTokenSource InternalCancellationSource, CancellationTokenSource LinkedCancellationTokenSource)
{
    public ItemStatus Status { get; internal set; }
}
