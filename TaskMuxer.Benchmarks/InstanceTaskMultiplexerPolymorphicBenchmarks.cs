namespace TaskMuxer.Benchmarks;

[MemoryDiagnoser(true)]
[ThreadingDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[BenchmarkCategory(new[] { nameof(InstanceTaskMultiplexer), "Polymorphic" })]
[WarmupCount(2)]
[IterationCount(3)]
[RPlotExporter]
public class InstanceTaskMultiplexerPolymorphicBenchmarks
{
    private static InstanceTaskMultiplexer ServiceFactoryNoLogger => new();
    private static InstanceTaskMultiplexer ServiceFactoryILogger => new(logger: new MockLogger<InstanceTaskMultiplexer>());

    private static int result0 = 1;
    private static string result1 = "hello";
    private static bool result2 = true;
    private static (int, string) result3 = (1, "hello");
    private static dynamic result4 = new { Test = 1 };

    private static Func<CancellationToken, Task<T?>> GetCompletedWorkFuncFactory<T>(int kind = 0) where T : notnull =>
        _ => Task.FromResult<T?>(kind switch
        {
            0 => result0,
            1 => result1,
            2 => result2,
            3 => result3,
            _ => result4
        }
    );

    private static Task AddCompletedWorkFuncFactory(InstanceTaskMultiplexer service, int index, string key, bool withItemKey = false)
    {
        var kind = index % 5;
        return kind switch
        {
            0 => withItemKey switch
            {
                true => service.AddTask<int>(new ItemKey(key, typeof(int)), GetCompletedWorkFuncFactory<int>(kind)),
                _ => service.AddTask<int>(key, GetCompletedWorkFuncFactory<int>(kind)),
            },
            1 => withItemKey switch
            {
                true => service.AddTask<string>(new ItemKey(key, typeof(string)), GetCompletedWorkFuncFactory<string>(kind)),
                _ => service.AddTask<string>(key, GetCompletedWorkFuncFactory<string>(kind)),
            },
            2 => withItemKey switch
            {
                true => service.AddTask<bool>(new ItemKey(key, typeof(bool)), GetCompletedWorkFuncFactory<bool>(kind)),
                _ => service.AddTask<bool>(key, GetCompletedWorkFuncFactory<bool>(kind)),
            },
            3 => withItemKey switch
            {
                true => service.AddTask<(int, string)>(new ItemKey(key, typeof((int, string))), GetCompletedWorkFuncFactory<(int, string)>(kind)),
                _ => service.AddTask<(int, string)>(key, GetCompletedWorkFuncFactory<(int, string)>(kind)),
            },
            _ => withItemKey switch
            {
                true => service.AddTask<dynamic>(new ItemKey(key, result4.GetType()), GetCompletedWorkFuncFactory<dynamic>(kind)),
                _ => service.AddTask<dynamic>(key, GetCompletedWorkFuncFactory<dynamic>(kind)),
            }
        };
    }

    private static Func<CancellationToken, Task<T?>> GetFuncFactory<T>(int kind = 0, int delay = 500) where T : notnull =>
        ct => Task.Run<T?>(async () =>
        {
            await Task.Delay(delay, ct);
            return (kind % 5) switch
            {
                0 => result0,
                1 => result1,
                2 => result2,
                3 => result3,
                _ => result4
            };
        }, ct);

    private static Task AddFuncFactory(InstanceTaskMultiplexer service, int index, string key, bool withItemKey = false)
    {
        var kind = index % 5;
        return kind switch
        {
            0 => withItemKey switch
            {
                true => service.AddTask<int>(new ItemKey(key, typeof(int)), GetFuncFactory<int>(kind)),
                _ => service.AddTask<int>(key, GetFuncFactory<int>(kind)),
            },
            1 => withItemKey switch
            {
                true => service.AddTask<string>(new ItemKey(key, typeof(string)), GetFuncFactory<string>(kind)),
                _ => service.AddTask<string>(key, GetFuncFactory<string>(kind)),
            },
            2 => withItemKey switch
            {
                true => service.AddTask<bool>(new ItemKey(key, typeof(bool)), GetFuncFactory<bool>(kind)),
                _ => service.AddTask<bool>(key, GetFuncFactory<bool>(kind)),
            },
            3 => withItemKey switch
            {
                true => service.AddTask<(int, string)>(new ItemKey(key, typeof((int, string))), GetFuncFactory<(int, string)>(kind)),
                _ => service.AddTask<(int, string)>(key, GetFuncFactory<(int, string)>(kind)),
            },
            _ => withItemKey switch
            {
                true => service.AddTask<dynamic>(new ItemKey(key, result4.GetType()), GetFuncFactory<dynamic>(kind)),
                _ => service.AddTask<dynamic>(key, GetFuncFactory<dynamic>(kind)),
            }
        };
    }

    private static async Task IndividualCompletedTasksWithKeyLoad(InstanceTaskMultiplexer service, int count)
    {
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddCompletedWorkFuncFactory(service, x, "test" + x)));
    }

    private static async Task IndividualCompletedTasksWithItemKeyLoad(InstanceTaskMultiplexer service, int count)
    {
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddCompletedWorkFuncFactory(service, x, "test" + x, true)));
    }

    private static async Task IndividualTasksWithKeyLoad(InstanceTaskMultiplexer service, int count)
    {
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddFuncFactory(service, x, "test" + x)));
    }

    private static async Task IndividualTasksWithItemKeyLoad(InstanceTaskMultiplexer service, int count)
    {
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddFuncFactory(service, x, "test" + x, true)));
    }

    private static async Task ConcurrentTasksWithKeyLoad(InstanceTaskMultiplexer service, int count, int concurrent)
    {
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddFuncFactory(service, x, "test" + (x % concurrent))));
    }
    private static async Task ConcurrentTasksWithItemKeyLoad(InstanceTaskMultiplexer service, int count, int concurrent)
    {
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddFuncFactory(service, x, "test" + (x % concurrent), true)));
    }

    [Benchmark]
    public async Task Single_Completed_Task_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryNoLogger, 1);

    [Benchmark]
    public async Task Single_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1);

    [Benchmark]
    public async Task Single_Completed_Task_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryILogger, 1);

    [Benchmark]
    public async Task Single_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryILogger, 1);

    [Benchmark]
    public async Task Single_Task_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryNoLogger, 1);

    [Benchmark]
    public async Task Single_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1);

    [Benchmark]
    public async Task Single_Task_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryILogger, 1);

    [Benchmark]
    public async Task Single_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryILogger, 1);

    [Benchmark]
    public async Task Five_Different_Completed_Task_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryNoLogger, 5);

    [Benchmark]
    public async Task Five_Different_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryNoLogger, 5);

    [Benchmark]
    public async Task Five_Different_Completed_Task_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryILogger, 5);

    [Benchmark]
    public async Task Five_Different_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryILogger, 5);

    [Benchmark]
    public async Task Five_Different_Task_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryNoLogger, 5);

    [Benchmark]
    public async Task Five_Different_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryNoLogger, 5);

    [Benchmark]
    public async Task Five_Different_Task_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryILogger, 5);

    [Benchmark]
    public async Task Five_Different_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryILogger, 5);

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Task_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryNoLogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryNoLogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Task_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryILogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryILogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Task_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryNoLogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryNoLogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Task_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryILogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryILogger, 100);

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await ConcurrentTasksWithKeyLoad(ServiceFactoryNoLogger, 100, 10);

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await ConcurrentTasksWithItemKeyLoad(ServiceFactoryNoLogger, 100, 10);

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await ConcurrentTasksWithKeyLoad(ServiceFactoryILogger, 100, 10);

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await ConcurrentTasksWithItemKeyLoad(ServiceFactoryILogger, 100, 10);

    [Benchmark]
    public async Task One_Thousand_Different_Completed_Task_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryNoLogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Completed_Task_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryILogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryILogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Task_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryNoLogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Task_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryILogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryILogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Tasks_With_One_Hundred_Concurrent_Per_Key_Of_Polymorphic_Types_With_Key_And_No_Logger() =>
        await ConcurrentTasksWithKeyLoad(ServiceFactoryNoLogger, 1_000, 100);

    [Benchmark]
    public async Task One_Thousand_Tasks_With_One_Hundred_Concurrent_Per_Key_Of_Polymorphic_Types_With_ItemKey_And_No_Logger() =>
        await ConcurrentTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1_000, 100);

    [Benchmark]
    public async Task One_Thousand_Tasks_With_One_Hundred_Concurrent_Per_Key_Of_Polymorphic_Types_With_Key_And_ILogger() =>
        await ConcurrentTasksWithKeyLoad(ServiceFactoryILogger, 1_000, 100);

    [Benchmark]
    public async Task One_Thousand_Tasks_With_One_Hundred_Concurrent_Per_Key_Of_Polymorphic_Types_With_ItemKey_And_ILogger() =>
        await ConcurrentTasksWithItemKeyLoad(ServiceFactoryILogger, 1_000, 100);
}
