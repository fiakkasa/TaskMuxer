using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TaskMuxer;

public static class Extensions
{
    internal static IServiceCollection AddValidatedOptions<T>(this IServiceCollection services, string? sectionKey = default) where T : class =>
        services
            .AddOptions<T>()
            .BindConfiguration(sectionKey ?? typeof(T).Name)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Services;

    public static IServiceCollection AddInstanceTaskMultiplexerNoLogger(this IServiceCollection services) =>
        services.AddSingleton<ITaskMultiplexer>(new InstanceTaskMultiplexer());

    public static IServiceCollection AddInstanceTaskMultiplexerWithILogger(this IServiceCollection services) =>
        services.AddSingleton<ITaskMultiplexer>(sp => new InstanceTaskMultiplexer(logger: sp.GetRequiredService<ILogger<InstanceTaskMultiplexer>>()));

    public static IServiceCollection AddInstanceTaskMultiplexerWithILoggerFactory(this IServiceCollection services) =>
        services.AddSingleton<ITaskMultiplexer>(sp => new InstanceTaskMultiplexer(logger: sp.GetRequiredService<ILoggerFactory>().CreateLogger<InstanceTaskMultiplexer>()));

    public static IServiceCollection AddInstanceTaskMultiplexerWithOptionsAndNoLogger(this IServiceCollection services, string? sectionKey = default) =>
        services
            .AddValidatedOptions<InstanceTaskMultiplexerConfig>(sectionKey)
            .AddSingleton<ITaskMultiplexer>(sp => new InstanceTaskMultiplexer(sp.GetRequiredService<IOptionsMonitor<InstanceTaskMultiplexerConfig>>().CurrentValue));

    public static IServiceCollection AddInstanceTaskMultiplexerWithOptionsAndILogger(this IServiceCollection services, string? sectionKey = default) =>
        services
            .AddValidatedOptions<InstanceTaskMultiplexerConfig>(sectionKey)
            .AddSingleton<ITaskMultiplexer>(sp =>
                new InstanceTaskMultiplexer(
                    sp.GetRequiredService<IOptionsMonitor<InstanceTaskMultiplexerConfig>>().CurrentValue,
                    sp.GetRequiredService<ILogger<InstanceTaskMultiplexer>>()
                )
            );

    public static IServiceCollection AddInstanceTaskMultiplexerWithOptionsAndILoggerFactory(this IServiceCollection services, string? sectionKey = default) =>
        services
            .AddValidatedOptions<InstanceTaskMultiplexerConfig>(sectionKey)
            .AddSingleton<ITaskMultiplexer>(sp =>
                new InstanceTaskMultiplexer(
                    sp.GetRequiredService<IOptionsMonitor<InstanceTaskMultiplexerConfig>>().CurrentValue,
                    sp.GetRequiredService<ILoggerFactory>().CreateLogger<InstanceTaskMultiplexer>()
                )
            );
}
