using Castle.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TaskMuxer.Tests;

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

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_No_Logger_And_Options() =>
        Assert.IsAssignableFrom<ITaskMultiplexer>(
            new ServiceCollection()
                .AddSingleton(Substitute.For<IOptionsMonitor<InstanceTaskMultiplexerConfig>>())
                .AddSingleton(Substitute.For<IConfiguration>())
                .AddInstanceTaskMultiplexerWithOptionsAndNoLogger()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>()
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_Logger_And_Options() =>
        Assert.IsAssignableFrom<ITaskMultiplexer>(
            new ServiceCollection()
                .AddLogging()
                .AddSingleton(Substitute.For<IOptionsMonitor<InstanceTaskMultiplexerConfig>>())
                .AddSingleton(Substitute.For<IConfiguration>())
                .AddInstanceTaskMultiplexerWithOptionsAndILogger()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>()
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_ILoggerFactory_And_Options() =>
        Assert.IsAssignableFrom<ITaskMultiplexer>(
            new ServiceCollection()
                .AddLogging()
                .AddSingleton(Substitute.For<IOptionsMonitor<InstanceTaskMultiplexerConfig>>())
                .AddSingleton(Substitute.For<IConfiguration>())
                .AddInstanceTaskMultiplexerWithOptionsAndILoggerFactory()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>()
        );
}
