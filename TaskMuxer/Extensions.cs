﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TaskMuxer;

public static class Extensions
{
    public static IServiceCollection AddInstanceTaskMultiplexerNoLogger(this IServiceCollection services) =>
        services.AddSingleton<ITaskMultiplexer>(new InstanceTaskMultiplexer());

    public static IServiceCollection AddInstanceTaskMultiplexerWithILogger(this IServiceCollection services) =>
        services.AddSingleton<ITaskMultiplexer>(sp => new InstanceTaskMultiplexer(sp.GetRequiredService<ILogger<InstanceTaskMultiplexer>>()));

    public static IServiceCollection AddInstanceTaskMultiplexerWithILoggerFactory(this IServiceCollection services) =>
        services.AddSingleton<ITaskMultiplexer>(sp => new InstanceTaskMultiplexer(sp.GetRequiredService<ILoggerFactory>()));
}