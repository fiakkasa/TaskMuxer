using Castle.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TaskMuxer.Tests;

public class ExtensionsTests
{
    [Fact]
    public void Register_InstanceTaskMultiplexer_With_No_Logger() =>
        Assert.IsType<ITaskMultiplexer>(
            new ServiceCollection()
                .AddInstanceTaskMultiplexerNoLogger()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>(), 
            exactMatch: false
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_ILogger() =>
        Assert.IsType<ITaskMultiplexer>(
            new ServiceCollection()
                .AddLogging()
                .AddInstanceTaskMultiplexerWithILogger()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>(), 
            exactMatch: false
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_ILoggerFactory() =>
        Assert.IsType<ITaskMultiplexer>(
            new ServiceCollection()
                .AddLogging()
                .AddInstanceTaskMultiplexerWithILoggerFactory()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>(),
            exactMatch: false
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_No_Logger_And_Options() =>
        Assert.IsType<ITaskMultiplexer>(
            new ServiceCollection()
                .AddSingleton(Substitute.For<IOptionsMonitor<InstanceTaskMultiplexerConfig>>())
                .AddSingleton(Substitute.For<IConfiguration>())
                .AddInstanceTaskMultiplexerWithOptionsAndNoLogger()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>(), 
            exactMatch: false
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_No_Logger_Options_And_Section_Key() =>
        Assert.IsType<ITaskMultiplexer>(
            new ServiceCollection()
                .AddSingleton(Substitute.For<IOptionsMonitor<InstanceTaskMultiplexerConfig>>())
                .AddSingleton(Substitute.For<IConfiguration>())
                .AddInstanceTaskMultiplexerWithOptionsAndNoLogger(sectionKey: "TestSection")
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>(), 
            exactMatch: false
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_Logger_And_Options() =>
        Assert.IsType<ITaskMultiplexer>(
            new ServiceCollection()
                .AddLogging()
                .AddSingleton(Substitute.For<IOptionsMonitor<InstanceTaskMultiplexerConfig>>())
                .AddSingleton(Substitute.For<IConfiguration>())
                .AddInstanceTaskMultiplexerWithOptionsAndILogger()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>(), 
            exactMatch: false
        );

    [Fact]
    public void Register_InstanceTaskMultiplexer_With_ILoggerFactory_And_Options() =>
        Assert.IsType<ITaskMultiplexer>(
            new ServiceCollection()
                .AddLogging()
                .AddSingleton(Substitute.For<IOptionsMonitor<InstanceTaskMultiplexerConfig>>())
                .AddSingleton(Substitute.For<IConfiguration>())
                .AddInstanceTaskMultiplexerWithOptionsAndILoggerFactory()
                .BuildServiceProvider()
                .GetRequiredService<ITaskMultiplexer>(), 
            exactMatch: false
        );
}
