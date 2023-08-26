using Microsoft.Extensions.DependencyInjection;

namespace TaskMultiplexer.Tests;

public class ExtensionsTests
{
    [Fact]
    public void Register_InstanceTaskMultiplexer_With_No_Logger() =>
        Assert.IsAssignableFrom<ITaskMultiplexer>(
            new ServiceCollection()
                .AddInstanceTaskMultiplexerNoLogger()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>()
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_ILogger() =>
        Assert.IsAssignableFrom<ITaskMultiplexer>(
            new ServiceCollection()
                .AddLogging()
                .AddInstanceTaskMultiplexerWithILogger()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>()
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_ILoggerFactory() =>
        Assert.IsAssignableFrom<ITaskMultiplexer>(
            new ServiceCollection()
                .AddLogging()
                .AddInstanceTaskMultiplexerWithILoggerFactory()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>()
        );
}