using System.Diagnostics.CodeAnalysis;

namespace TaskMultiplexer;

[ExcludeFromCodeCoverage]
public record ItemKey(string Key, Type Type);
