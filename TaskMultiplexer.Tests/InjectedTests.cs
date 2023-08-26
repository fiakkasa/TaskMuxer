using Microsoft.Extensions.DependencyInjection;

namespace TaskMultiplexer.Tests;

public class InjectedTests
{
    public class ItemsCountService
    {
        readonly ITaskMultiplexer _taskMultiplexer;

        public ItemsCountService(ITaskMultiplexer taskMultiplexer) => _taskMultiplexer = taskMultiplexer;

        public async Task<long> GetCountOfItems(CancellationToken cancellationToken = default) =>
            await _taskMultiplexer.AddTask(
                "items_count",
                async ct =>
                {
                    await Task.Delay(250, ct);
                    return Random.Shared.Next();
                },
                cancellationToken
            );
    }

    [Fact]
    public async Task Service_Returns_Results()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddInstanceTaskMultiplexerWithILogger()
            .AddSingleton<ItemsCountService>()
            .BuildServiceProvider();

        var service = serviceProvider.GetRequiredService<ItemsCountService>();

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select((_, __) => service.GetCountOfItems()));

        Assert.Equal(10, results.Length);
        Assert.Single(results.Distinct());
    }
}