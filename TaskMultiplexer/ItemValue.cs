using System.Diagnostics.CodeAnalysis;

namespace TaskMultiplexer;

[ExcludeFromCodeCoverage]
internal record ItemValue(object Value)
{
    public ItemStatus Status { get; internal set; }
}
