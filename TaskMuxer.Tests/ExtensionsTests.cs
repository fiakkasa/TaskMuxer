using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    public async Task Register_InstanceTaskMultiplexer_With_Options_Fails_When_Invalid_Config_NoLogger() =>
        await Assert.ThrowsAsync<OptionsValidationException>(async () =>
            await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                    webBuilder
                        .UseTestServer()
                        .ConfigureAppConfiguration(config =>
                            config.AddToConfigBuilder(
                                new Dictionary<string, object>()
                                {
                                    [nameof(InstanceTaskMultiplexerConfig)] = new InstanceTaskMultiplexerConfig
                                    {
                                        ExecutionTimeout = TimeSpan.Zero
                                    }
                                }
                            )
                        )
                        .ConfigureServices(services =>
                            services.AddInstanceTaskMultiplexerWithOptionsAndNoLogger()
                        )
                        .Configure(_ => { })
            )
            .StartAsync()
        );

    [Fact]
    public async Task Register_InstanceTaskMultiplexer_With_Custom_Options_Section_And_No_Logger()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
                webBuilder
                    .UseTestServer()
                    .ConfigureAppConfiguration(config =>
                        config.AddToConfigBuilder(
                            new Dictionary<string, object>()
                            {
                                ["ITMC"] = new InstanceTaskMultiplexerConfig
                                {
                                    ExecutionTimeout = TimeSpan.FromMilliseconds(250)
                                }
                            }
                        )
                    )
                    .ConfigureServices(services =>
                        services.AddInstanceTaskMultiplexerWithOptionsAndNoLogger("ITMC")
                    )
                    .Configure(_ => { })
           )
           .StartAsync();

        var service = host.Services.GetRequiredService<ITaskMultiplexer>();
        Assert.IsAssignableFrom<ITaskMultiplexer>(service);
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await service.AddTask(
            "test",
            async (ct) =>
            {
                await Task.Delay(1_000, ct);

                return 1;
            })
        );
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
                                [nameof(InstanceTaskMultiplexerConfig)] = new InstanceTaskMultiplexerConfig()
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
                                [nameof(InstanceTaskMultiplexerConfig)] = new InstanceTaskMultiplexerConfig()
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
