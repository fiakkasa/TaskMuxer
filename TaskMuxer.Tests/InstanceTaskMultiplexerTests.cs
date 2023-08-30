using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TaskMuxer.Tests;

public class InstanceTaskMultiplexerTests
{
    public static InstanceTaskMultiplexer ServiceFactoryNoLogger => new();
    public static InstanceTaskMultiplexer ServiceFactoryILogger => new(Substitute.For<ILogger<InstanceTaskMultiplexer>>());

    [Fact]
    public async Task ItemsCount_When_No_Items_Present() =>
        Assert.Equal(0, await ServiceFactoryNoLogger.ItemsCount());

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

        Assert.Equal(1, results.First());
    }

    [Fact]
    public async Task ItemKeys_When_No_Items_Present() =>
        Assert.Empty(await ServiceFactoryNoLogger.ItemKeys());

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

        Assert.Equal(new("items", typeof(int)), results.First());
    }

    [Fact]
    public async Task Get_Task_Status_When_Item_Does_Not_Exist() =>
        Assert.Equal(ItemStatus.None, await ServiceFactoryNoLogger.GetTaskStatus<int>("status"));

    [Fact]
    public async Task Get_Task_Status_When_Item_Exists()
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

        Assert.Equal(ItemStatus.Started, results.First());
    }

    [Fact]
    public async Task Get_Task_Status_With_ItemKey_When_Item_Does_Not_Exist() =>
        Assert.Equal(ItemStatus.None, await ServiceFactoryNoLogger.GetTaskStatus(new("status", typeof(int))));

    [Fact]
    public async Task Get_Task_Status_With_ItemKey_When_Item_Exists()
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
                results.Add(await service.GetTaskStatus(new("status", typeof(int))));
            })
        );

        Assert.Equal(ItemStatus.Started, results.First());
    }

    [Fact]
    public async Task Has_Task_When_Item_Does_Not_Exist() =>
        Assert.False(await ServiceFactoryNoLogger.HasTask<int>("has"));

    [Fact]
    public async Task Has_Task_When_Item_Exists()
    {
        var service = ServiceFactoryNoLogger;
        var results = new ConcurrentBag<bool>();

        await Task.WhenAll(
            service.AddTask(
                "has",
                async ct =>
                {
                    await Task.Delay(500, ct);
                    return 1;
                }
            ),
            Task.Run(async () =>
            {
                await Task.Delay(250);
                results.Add(await service.HasTask<int>("has"));
            })
        );

        Assert.True(results.First());
    }

    [Fact]
    public async Task Has_Task_With_ItemKey_When_Item_Does_Not_Exist() =>
        Assert.False(await ServiceFactoryNoLogger.HasTask(new("has", typeof(int))));

    [Fact]
    public async Task Has_Task_With_ItemKey_When_Item_Exists()
    {
        var service = ServiceFactoryNoLogger;
        var results = new ConcurrentBag<bool>();

        await Task.WhenAll(
            service.AddTask(
                "has",
                async ct =>
                {
                    await Task.Delay(500, ct);
                    return 1;
                }
            ),
            Task.Run(async () =>
            {
                await Task.Delay(250);
                results.Add(await service.HasTask(new("has", typeof(int))));
            })
        );

        Assert.True(results.First());
    }

    [Fact]
    public async Task Get_Task_When_Item_Does_Not_Exist() =>
        Assert.Null(await ServiceFactoryNoLogger.GetTask<int>("get"));

    [Fact]
    public async Task Get_Task_When_Item_Exists()
    {
        var service = ServiceFactoryNoLogger;
        var results = new ConcurrentBag<object?>();

        await Task.WhenAll(
            service.AddTask(
                "get",
                async ct =>
                {
                    await Task.Delay(500, ct);
                    return 1;
                }
            ),
            Task.Run(async () =>
            {
                await Task.Delay(250);
                results.Add(await service.GetTask<int>("get"));
            })
        );

        Assert.Single(results);
    }

    [Fact]
    public async Task Get_Task_With_ItemKey_When_Item_Does_Not_Exist() =>
        Assert.Null(await ServiceFactoryNoLogger.GetTask<int>(new ItemKey("get", typeof(int))));

    [Fact]
    public async Task Get_Task_With_ItemKey_When_Item_Exists()
    {
        var service = ServiceFactoryNoLogger;
        var results = new ConcurrentBag<object?>();

        await Task.WhenAll(
            service.AddTask(
                "get",
                async ct =>
                {
                    await Task.Delay(500, ct);
                    return 1;
                }
            ),
            Task.Run(async () =>
            {
                await Task.Delay(250);
                results.Add(await service.GetTask<int>(new ItemKey("get", typeof(int))));
            })
        );

        Assert.Single(results);
    }

    [Fact]
    public async Task Add_Task_With_ItemKey_Logs()
    {
        var logger = Substitute.For<ILogger<InstanceTaskMultiplexer>>();
        var service = new InstanceTaskMultiplexer(logger);
        var key = new ItemKey("log", typeof(int));

        Assert.Equal(
            1,
            await service.AddTask(
                "log",
                async ct =>
                {
                    await Task.Delay(250, ct);
                    return 1;
                }
            )
        );

        var calls = logger.ReceivedCalls().ToList();

        Assert.Single(
            calls.Where(x =>
                x.GetArguments()
                    .Where(y =>
                        y is IEnumerable<KeyValuePair<string, object>> args
                        && y.ToString() == $"Request with key {key} was added to the items list"
                    )
                    .Any()
            )
        );

        Assert.Single(
             calls.Where(x =>
                 x.GetArguments()
                     .Where(y =>
                         y is IEnumerable<KeyValuePair<string, object>>
                         && y?.ToString() == $"Number of items in list: {1}"
                     )
                     .Any()
             )
         );
        Assert.Single(
            calls.Where(x =>
                x.GetArguments()
                    .Where(y =>
                        y is IEnumerable<KeyValuePair<string, object>> args
                        && args.LastOrDefault().Value?.ToString() is { } originalMessage
                        && originalMessage == "Request with key {Key} has completed at {Timestamp}, after {TimeElapsed}, {ResultMessage}, and will be removed from the items list"
                        && y.ToString() is { } message
                        && message.Contains(" successfully,")
                    )
                    .Any()
            )
        );
        Assert.Single(
            calls.Where(x =>
                x.GetArguments()
                    .Where(y =>
                        y is IEnumerable<KeyValuePair<string, object>>
                        && y?.ToString() == $"Number of items remaining in the list: {0}"
                    )
                    .Any()
            )
        );
    }

    [Fact]
    public async Task Add_Task_With_ItemKey() =>
        Assert.Equal(
            1,
            await ServiceFactoryNoLogger.AddTask(
                "status",
                async ct =>
                {
                    await Task.Delay(250, ct);
                    return 1;
                }
            )
        );

    [Fact]
    public async Task Add_Task_Single_Type_Items_With_Cancellation_Set_To_Expire_Before_Completion()
    {
        var service = ServiceFactoryNoLogger;
        var count = 0;
        var maxConcurrentItems = 0L;
        var cts = new CancellationTokenSource();
        cts.CancelAfter(375);

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await Task.WhenAll(
                Enumerable.Range(1, 3).Select((v, i) =>
                    service.AddTask(
                        "whenAll",
                        async ct =>
                        {
                            var itemsCount = await service.ItemsCount(ct);
                            if (itemsCount > maxConcurrentItems)
                                Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                            await Task.Delay(1_000, ct);

                            Interlocked.Increment(ref count);

                            return Random.Shared.Next();
                        },
                        cts.Token
                    )
                )
            )
        );
        Assert.Equal(0, count);
        Assert.Equal(1, maxConcurrentItems);
    }

    [Fact]
    public async Task Add_Task_Polymorphic_Items_With_Cancellation_Set_To_Expire_Before_Completion()
    {
        var service = ServiceFactoryNoLogger;
        int count = 0;
        var maxConcurrentItems = 0L;
        var cts = new CancellationTokenSource();
        var results = new ConcurrentBag<object>();
        cts.CancelAfter(375);

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await Task.WhenAll(
                // added
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        var itemsCount = await service.ItemsCount(ct);
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        await Task.Delay(250, ct);

                        results.Add("banana");

                        Interlocked.Increment(ref count);

                        return "banana";
                    },
                    cts.Token
                ),
                // skipped
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        var itemsCount = await service.ItemsCount(ct);
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        await Task.Delay(500, ct);
                        results.Add("banana");
                        Interlocked.Increment(ref count);

                        return "banana";
                    },
                    cts.Token
                ),
                // added - cancelled
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        var itemsCount = await service.ItemsCount(ct);
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        await Task.Delay(500, ct);
                        results.Add(true);
                        Interlocked.Increment(ref count);

                        return true;
                    },
                    cts.Token
                ),
                // added
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        var itemsCount = await service.ItemsCount(ct);
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        await Task.Delay(250, ct);
                        results.Add(("hello", "world"));
                        Interlocked.Increment(ref count);

                        return ("hello", "world");
                    },
                    cts.Token
                ),
                // skipped
                service.AddTask(
                    "cancellable",
                    async (CancellationToken ct) =>
                    {
                        var itemsCount = await service.ItemsCount(ct);
                        if (itemsCount > maxConcurrentItems)
                            Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                        await Task.Delay(250, ct);
                        results.Add(true);
                        Interlocked.Increment(ref count);

                        return true;
                    },
                    cts.Token
                )
            )
        );

        Assert.Equal(2, count);
        Assert.Equal(3, maxConcurrentItems);
        Assert.Equal(2, results.Count);
        Assert.Single(results.OfType<string>().Where(x => x == "banana"));
        Assert.Single(results.OfType<(string, string)>().Where(x => x == ("hello", "world")));
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_And_Extreme_Number_Of_Requests_With_ILogger()
    {
        var service = ServiceFactoryILogger;
        var count = 0;
        var maxConcurrentItems = 0L;
        var results = (
                await Task.WhenAll(
                    Enumerable.Range(1, 1_000_000).Select((_, __) => service.AddTask(
                        "whenAll",
                        async ct =>
                        {
                            var itemsCount = await service.ItemsCount(ct);
                            if (itemsCount > maxConcurrentItems)
                                Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                            await Task.Delay(1_000, ct);
                            Interlocked.Increment(ref count);

                            return Random.Shared.Next();
                        }
                    )
                )
            )
        )
        .Distinct()
        .ToList();

        Assert.InRange(count, 1, 10);
        Assert.Equal(1, maxConcurrentItems);
        Assert.Equal(results.Count, count);
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_And_Extreme_Number_Of_Requests_With_ILogger_Using_ParallelForEach()
    {
        var service = ServiceFactoryILogger;
        var count = 0;
        var maxConcurrentItems = 0L;
        var results = new ConcurrentBag<int>();

        await Parallel.ForEachAsync(
            Enumerable.Range(1, 1_000_000),
            async (_, cato) => await service.AddTask(
                "forEach",
                async ct =>
                {
                    var itemsCount = await service.ItemsCount(ct);
                    if (itemsCount > maxConcurrentItems)
                        Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                    await Task.Delay(500, ct);
                    var result = Random.Shared.Next();
                    results.Add(result);
                    Interlocked.Increment(ref count);

                    return result;
                },
                cato
            )
        );

        Assert.InRange(count, 1, 10);
        Assert.Equal(1, maxConcurrentItems);
        Assert.Equal(results.Count, count);
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_And_Extreme_Number_Of_Requests_With_No_Logger_Using_ParallelForEach()
    {
        var service = ServiceFactoryNoLogger;
        var count = 0;
        var maxConcurrentItems = 0L;
        var results = new ConcurrentBag<int>();

        await Parallel.ForEachAsync(
            Enumerable.Range(1, 1_000_000),
            async (_, cato) => await service.AddTask(
                "forEach",
                async ct =>
                {
                    var itemsCount = await service.ItemsCount(ct);
                    if (itemsCount > maxConcurrentItems)
                        Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                    await Task.Delay(500, ct);
                    var result = Random.Shared.Next();
                    results.Add(result);
                    Interlocked.Increment(ref count);

                    return result;
                },
                cato
            )
        );

        Assert.InRange(count, 1, 10);
        Assert.Equal(1, maxConcurrentItems);
        Assert.Equal(results.Count, count);
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_And_Large_Number_Of_Requests_With_ILogger_Using_ParallelForEach()
    {
        var service = ServiceFactoryILogger;
        var count = 0;
        var maxConcurrentItems = 0L;
        var results = new ConcurrentBag<int>();

        await Parallel.ForEachAsync(
            Enumerable.Range(1, 1_000),
            async (_, cato) => await service.AddTask(
                "forEach",
                async ct =>
                {
                    var itemsCount = await service.ItemsCount(ct);
                    if (itemsCount > maxConcurrentItems)
                        Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                    await Task.Delay(1_000, ct);
                    results.Add(Random.Shared.Next());
                    Interlocked.Increment(ref count);

                    return 1;
                },
                cato
            )
        );

        Assert.InRange(count, 1, 10);
        Assert.Equal(1, maxConcurrentItems);
        Assert.Equal(results.Count, count);
    }

    [Fact]
    public async Task Add_Task_Single_Type_Items_And_Large_Number_Of_Unique_And_Parallel_Requests_With_ILogger_Using_ParallelForEach_And_WhenAll()
    {
        var service = ServiceFactoryILogger;
        var count = 0;
        var maxConcurrentItems = 0L;

        await Parallel.ForEachAsync(
            Enumerable.Range(1, 100),
            new ParallelOptions { MaxDegreeOfParallelism = 8 },
            async (v, __) => await Task.WhenAll(
                    Enumerable.Range(1, 9).Select(vi => service.AddTask(
                        "forEach_and_whenAll_" + v + "_" + (vi % 3),
                        async ct =>
                        {
                            var itemsCount = await service.ItemsCount(ct);
                            if (itemsCount > maxConcurrentItems)
                                Interlocked.Exchange(ref maxConcurrentItems, itemsCount);

                            await Task.Delay(500, ct);
                            Interlocked.Increment(ref count);

                            return Random.Shared.Next();
                        }
                    )
                )
            )
        );

        Assert.Equal(300, count);
        Assert.InRange(maxConcurrentItems, 20, 40);
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
