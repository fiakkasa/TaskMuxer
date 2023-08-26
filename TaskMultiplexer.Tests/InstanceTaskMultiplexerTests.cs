using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TaskMultiplexer.Tests;

public class InstanceTaskMultiplexerTests
{
    public InstanceTaskMultiplexer ServiceFactoryNoLogger => new();
    public InstanceTaskMultiplexer ServiceFactoryILogger => new(Substitute.For<ILogger<InstanceTaskMultiplexer>>());
    public InstanceTaskMultiplexer ServiceFactoryILoggerFactory => new(Substitute.For<ILoggerFactory>());

    [Fact]
    public async Task On_Init_No_Items_Present()
    {
        var service = ServiceFactoryNoLogger;

        Assert.Equal(0, await service.ItemsCount());
        Assert.Empty(await service.ItemKeys());
        Assert.Equal(ItemStatus.None, await service.GetTaskStatus<int>("test"));
    }

    [Fact]
    public async Task ItemsCount_When_Items_Present()
    {
        var service = ServiceFactoryNoLogger;
        var results = new ConcurrentBag<long>();

        await Task.WhenAll(
            service.AddTask(
                "items",
                async ct =>
                {
                    await Task.Delay(500, ct);
                    return 1;
                }
            ),
            Task.Run(async () =>
            {
                await Task.Delay(250);
                results.Add(await service.ItemsCount());
            })
        );

        Assert.Contains(1, results);
    }

    [Fact]
    public async Task ItemKeys_When_Items_Present()
    {
        var service = ServiceFactoryNoLogger;
        var results = new ConcurrentBag<ItemKey>();

        await Task.WhenAll(
            service.AddTask(
                "items",
                async ct =>
                {
                    await Task.Delay(500, ct);
                    return 1;
                }
            ),
            Task.Run(async () =>
            {
                await Task.Delay(250);
                results.Add((await service.ItemKeys()).First());
            })
        );

        Assert.Contains(new("items", typeof(int)), results);
    }

    [Fact]
    public async Task Get_Task_Status()
    {
        var service = ServiceFactoryNoLogger;
        var results = new ConcurrentBag<ItemStatus>();

        await Task.WhenAll(
            service.AddTask(
                "status",
                async ct =>
                {
                    await Task.Delay(500, ct);
                    return 1;
                }
            ),
            Task.Run(async () =>
            {
                await Task.Delay(250);
                results.Add(await service.GetTaskStatus<int>("status"));
            })
        );

        Assert.Contains(ItemStatus.Started, results);
    }

    [Fact]
    public async Task Add_Task_Polymorphic_Items_With_Cancellation_Set_To_Expire_Before_Completion()
    {
        var service = ServiceFactoryNoLogger;
        int count = 0;
        var maxConcurrentItems = 0L;
        var cts = new CancellationTokenSource();
        var results = new ConcurrentBag<object>();
        cts.CancelAfter(300);

        try
        {
            await Task.WhenAll(
                // added
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        await Task.Delay(250, ct);
                        results.Add("banana");
                        Interlocked.Increment(ref count);

                        var itemsCount = await service.ItemsCount();
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        return "banana";
                    },
                    cts.Token
                ),
                // skipped
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        await Task.Delay(500, ct);
                        results.Add("banana");
                        Interlocked.Increment(ref count);

                        var itemsCount = await service.ItemsCount();
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        return "banana";
                    },
                    cts.Token
                ),
                // added - cancelled
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        await Task.Delay(500, ct);
                        results.Add(true);
                        Interlocked.Increment(ref count);

                        var itemsCount = await service.ItemsCount();
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        return true;
                    },
                    cts.Token
                ),
                // added
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        await Task.Delay(250, ct);
                        results.Add(("hello", "world"));
                        Interlocked.Increment(ref count);

                        var itemsCount = await service.ItemsCount();
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        return ("hello", "world");
                    },
                    cts.Token
                ),
                // skipped
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        await Task.Delay(250, ct);
                        results.Add(true);
                        Interlocked.Increment(ref count);

                        var itemsCount = await service.ItemsCount();
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        return true;
                    },
                    cts.Token
                )
            );
        }
        catch { }

        Assert.Equal(2, count);
        Assert.Equal(3, maxConcurrentItems);
        Assert.Equal(2, results.Count);
        Assert.Single(results.OfType<string>().Where(x => x == "banana"));
        Assert.Single(results.OfType<(string, string)>().Where(x => x == ("hello", "world")));
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_With_For_Each_And_Large_Number_Of_Requests_With_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        var count = 0;
        var maxConcurrentItems = 0L;
        var results = new ConcurrentBag<int>();

        await Parallel.ForEachAsync(
            Enumerable.Range(0, 1_000_000),
            async (_, cato) => await service.AddTask(
                "forEach",
                async ct =>
                {
                    await Task.Delay(2_000, ct);
                    results.Add(Random.Shared.Next());
                    Interlocked.Increment(ref count);

                    var itemsCount = await service.ItemsCount();
                    if (itemsCount > maxConcurrentItems)
                        Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                    return 1;
                },
                cato
            )
        );

        Assert.Equal(1, count);
        Assert.Equal(1, maxConcurrentItems);
        Assert.Single(results);
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_With_For_Each_And_Large_Number_Of_Requests_With_ILogger()
    {
        var service = ServiceFactoryILogger;
        var count = 0;
        var maxConcurrentItems = 0L;
        var results = new ConcurrentBag<int>();

        await Parallel.ForEachAsync(
            Enumerable.Range(0, 1_000),
            async (_, cato) => await service.AddTask(
                "forEachILogger",
                async ct =>
                {
                    await Task.Delay(1_000, ct);
                    results.Add(Random.Shared.Next());
                    Interlocked.Increment(ref count);

                    var itemsCount = await service.ItemsCount();
                    if (itemsCount > maxConcurrentItems)
                        Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                    return 1;
                },
                cato
            )
        );

        Assert.Equal(1, count);
        Assert.Equal(1, maxConcurrentItems);
        Assert.Single(results);
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_With_For_Each_And_Large_Number_Of_Requests_With_ILoggerFactory()
    {
        var service = ServiceFactoryILoggerFactory;
        var count = 0;
        var maxConcurrentItems = 0L;
        var results = new ConcurrentBag<int>();

        await Parallel.ForEachAsync(
            Enumerable.Range(0, 1_000),
            async (_, cato) => await service.AddTask(
                "forEach",
                async ct =>
                {
                    await Task.Delay(1_000, ct);
                    results.Add(Random.Shared.Next());
                    Interlocked.Increment(ref count);

                    var itemsCount = await service.ItemsCount();
                    if (itemsCount > maxConcurrentItems)
                        Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                    return 1;
                },
                cato
            )
        );

        Assert.Equal(1, count);
        Assert.Equal(1, maxConcurrentItems);
        Assert.Single(results);
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_With_WhenAll_And_Large_Number_Of_Requests()
    {
        var service = ServiceFactoryNoLogger;
        var count = 0;
        var maxConcurrentItems = 0L;
        var results = (
                await Task.WhenAll(
                    Enumerable.Range(0, 1_000).Select((_, __) => service.AddTask(
                        "whenAll",
                        async ct =>
                        {
                            await Task.Delay(1_000, ct);
                            Interlocked.Increment(ref count);

                            var itemsCount = await service.ItemsCount();
                            if (itemsCount > maxConcurrentItems)
                                Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                            return Random.Shared.Next();
                        }
                    )
                )
            )
        )
        .Distinct()
        .ToList();

        Assert.Equal(1, count);
        Assert.Equal(1, maxConcurrentItems);
        Assert.Single(results);
    }

    [Fact]
    public async Task Add_Task_Request_Throws_Exception_No_Logger() =>
        await Assert.ThrowsAsync<Exception>(async () =>
            await ServiceFactoryNoLogger.AddTask<int>(
                "exception",
                _ => throw new Exception("Splash")
            )
        );

    [Fact]
    public async Task Add_Task_Request_Throws_Exception_ILogger() =>
        await Assert.ThrowsAsync<Exception>(async () =>
            await ServiceFactoryILogger.AddTask<int>(
                "exception",
                _ => throw new Exception("Splash")
            )
        );

    [Fact]
    public async Task Add_Task_Request_Throws_Exception_ILoggerFactory() =>
        await Assert.ThrowsAsync<Exception>(async () =>
            await ServiceFactoryILoggerFactory.AddTask<int>(
                "exception",
                _ => throw new Exception("Splash")
            )
        );

    [Fact]
    public async Task Add_Task_Request_Cancelled()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await ServiceFactoryNoLogger.AddTask(
                "cancelled",
                async ct =>
                {
                    await Task.Delay(1_000, ct);

                    return 1;
                },
                cts.Token
            )
        );
    }
}
