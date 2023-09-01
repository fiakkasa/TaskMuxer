using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    public async Task Register_InstanceTaskMultiplexer_With_Options_And_No_Logger()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
                webBuilder
                    .UseTestServer()
                    .ConfigureAppConfiguration(config =>
                        config.AddToConfigBuilder(
                            new Dictionary<string, object>()
                            {
                                [nameof(InstanceTaskMultiplexer)] = new InstanceTaskMultiplexer()
                            }
                        )
                    )
                    .ConfigureServices(services =>
                        services.AddInstanceTaskMultiplexerWithOptionsAndNoLogger()
                    )
                    .Configure(_ => { })
           )
           .StartAsync();

        Assert.IsAssignableFrom<ITaskMultiplexer>(host.Services.GetRequiredService<ITaskMultiplexer>());
    }

    [Fact]
    public async Task Register_InstanceTaskMultiplexer_With_Options_And_ILogger()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
                webBuilder
                    .UseTestServer()
                    .ConfigureAppConfiguration(config =>
                        config.AddToConfigBuilder(
                            new Dictionary<string, object>()
                            {
                                [nameof(InstanceTaskMultiplexer)] = new InstanceTaskMultiplexer()
                            }
                        )
                    )
                    .ConfigureServices(services =>
                        services.AddInstanceTaskMultiplexerWithOptionsAndILogger()
                    )
                    .Configure(_ => { })
           )
           .StartAsync();

        Assert.IsAssignableFrom<ITaskMultiplexer>(host.Services.GetRequiredService<ITaskMultiplexer>());
    }

    [Fact]
    public async Task Register_InstanceTaskMultiplexer_With_Options_And_ILoggerFactory()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
                webBuilder
                    .UseTestServer()
                    .ConfigureAppConfiguration(config =>
                        config.AddToConfigBuilder(
                            new Dictionary<string, object>()
                            {
                                [nameof(InstanceTaskMultiplexer)] = new InstanceTaskMultiplexer()
                            }
                        )
                    )
                    .ConfigureServices(services =>
                        services.AddInstanceTaskMultiplexerWithOptionsAndILoggerFactory()
                    )
                    .Configure(_ => { })
           )
           .StartAsync();

        Assert.IsAssignableFrom<ITaskMultiplexer>(host.Services.GetRequiredService<ITaskMultiplexer>());
    }
}
