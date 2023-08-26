using System.Diagnostics.CodeAnalysis;

namespace TaskMuxer;

[ExcludeFromCodeCoverage]
public record ItemKey(string Key, Type Type);
