using System.Diagnostics.CodeAnalysis;

namespace TaskMuxer;

[ExcludeFromCodeCoverage]
internal record ItemValue(object Value)
{
    public ItemStatus Status { get; internal set; }
}
