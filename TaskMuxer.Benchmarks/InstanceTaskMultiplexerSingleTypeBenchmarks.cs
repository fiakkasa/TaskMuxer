namespace TaskMuxer.Benchmarks;

[MemoryDiagnoser(true)]
[ThreadingDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[BenchmarkCategory(nameof(InstanceTaskMultiplexer), "SingleType")]
[WarmupCount(2)]
[IterationCount(3)]
[RPlotExporter]
public class InstanceTaskMultiplexerSingleTypeBenchmarks
{
    private const int result0 = 0;
    private const int result1 = 1;
    private const int result2 = 2;
    private const int result3 = 3;
    private const int result4 = 4;
    private static InstanceTaskMultiplexer ServiceFactoryNoLogger => new();
    private static InstanceTaskMultiplexer ServiceFactoryILogger => new(logger: new MockLogger<InstanceTaskMultiplexer>());

    private static Func<CancellationToken, Task<int?>> GetCompletedWorkFuncFactory(int kind = 0) =>
        _ => Task.FromResult<int?>(kind switch
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
        return withItemKey switch
        {
            true => service.AddTask(new ItemKey(key, typeof(int)), GetCompletedWorkFuncFactory(kind)),
            _ => service.AddTask(key, GetCompletedWorkFuncFactory(kind))
        };
    }

    private static Func<CancellationToken, Task<int?>> GetFuncFactory(int kind = 0, int delay = 500) =>
        ct => Task.Run<int?>(async () =>
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
            },
            ct);

    private static Task AddFuncFactory(InstanceTaskMultiplexer service, int index, string key, bool withItemKey = false)
    {
        var kind = index % 5;
        return withItemKey switch
        {
            true => service.AddTask(new ItemKey(key, typeof(int)), GetFuncFactory(kind)),
            _ => service.AddTask(key, GetFuncFactory(kind))
        };
    }

    private static async Task IndividualCompletedTasksWithKeyLoad(InstanceTaskMultiplexer service, int count) =>
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddCompletedWorkFuncFactory(service, x, "test" + x)));

    private static async Task IndividualCompletedTasksWithItemKeyLoad(InstanceTaskMultiplexer service, int count) =>
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddCompletedWorkFuncFactory(service, x, "test" + x, true)));

    private static async Task IndividualTasksWithKeyLoad(InstanceTaskMultiplexer service, int count) =>
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddFuncFactory(service, x, "test" + x)));

    private static async Task IndividualTasksWithItemKeyLoad(InstanceTaskMultiplexer service, int count) =>
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddFuncFactory(service, x, "test" + x, true)));

    private static async Task ConcurrentTasksWithKeyLoad(InstanceTaskMultiplexer service, int count, int concurrent) =>
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddFuncFactory(service, x, "test" + (x % concurrent))));

    private static async Task ConcurrentTasksWithItemKeyLoad(InstanceTaskMultiplexer service, int count, int concurrent) =>
        await Task.WhenAll(Enumerable.Range(1, count).Select(x => AddFuncFactory(service, x, "test" + (x % concurrent), true)));

    [Benchmark]
    public async Task Single_Completed_Task_Of_Single_Type_With_Key_And_No_Logger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryNoLogger, 1);

    [Benchmark]
    public async Task Single_Completed_Task_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1);

    [Benchmark]
    public async Task Single_Completed_Task_Of_Single_Type_With_Key_And_ILogger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryILogger, 1);

    [Benchmark]
    public async Task Single_Completed_Task_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryILogger, 1);

    [Benchmark]
    public async Task Single_Task_Of_Single_Type_With_Key_And_No_Logger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryNoLogger, 1);

    [Benchmark]
    public async Task Single_Task_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1);

    [Benchmark]
    public async Task Single_Task_Of_Single_Type_With_Key_And_ILogger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryILogger, 1);

    [Benchmark]
    public async Task Single_Task_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryILogger, 1);

    [Benchmark]
    public async Task Five_Different_Completed_Task_Of_Single_Type_With_Key_And_No_Logger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryNoLogger, 5);

    [Benchmark]
    public async Task Five_Different_Completed_Task_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryNoLogger, 5);

    [Benchmark]
    public async Task Five_Different_Completed_Task_Of_Single_Type_With_Key_And_ILogger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryILogger, 5);

    [Benchmark]
    public async Task Five_Different_Completed_Task_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryILogger, 5);

    [Benchmark]
    public async Task Five_Different_Task_Of_Single_Type_With_Key_And_No_Logger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryNoLogger, 5);

    [Benchmark]
    public async Task Five_Different_Task_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryNoLogger, 5);

    [Benchmark]
    public async Task Five_Different_Task_Of_Single_Type_With_Key_And_ILogger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryILogger, 5);

    [Benchmark]
    public async Task Five_Different_Task_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryILogger, 5);

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Task_Of_Single_Type_With_Key_And_No_Logger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryNoLogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Task_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryNoLogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Task_Of_Single_Type_With_Key_And_ILogger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryILogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Task_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryILogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Task_Of_Single_Type_With_Key_And_No_Logger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryNoLogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Task_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryNoLogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Task_Of_Single_Type_With_Key_And_ILogger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryILogger, 100);

    [Benchmark]
    public async Task One_Hundred_Different_Task_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryILogger, 100);

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Single_Type_With_Key_And_No_Logger() =>
        await ConcurrentTasksWithKeyLoad(ServiceFactoryNoLogger, 100, 10);

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await ConcurrentTasksWithItemKeyLoad(ServiceFactoryNoLogger, 100, 10);

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Single_Type_With_Key_And_ILogger() =>
        await ConcurrentTasksWithKeyLoad(ServiceFactoryILogger, 100, 10);

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await ConcurrentTasksWithItemKeyLoad(ServiceFactoryILogger, 100, 10);

    [Benchmark]
    public async Task One_Thousand_Different_Completed_Task_Of_Single_Type_With_Key_And_No_Logger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryNoLogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Completed_Task_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Completed_Task_Of_Single_Type_With_Key_And_ILogger() =>
        await IndividualCompletedTasksWithKeyLoad(ServiceFactoryILogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Completed_Task_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await IndividualCompletedTasksWithItemKeyLoad(ServiceFactoryILogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Task_Of_Single_Type_With_Key_And_No_Logger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryNoLogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Task_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Task_Of_Single_Type_With_Key_And_ILogger() =>
        await IndividualTasksWithKeyLoad(ServiceFactoryILogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Different_Task_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await IndividualTasksWithItemKeyLoad(ServiceFactoryILogger, 1_000);

    [Benchmark]
    public async Task One_Thousand_Tasks_With_One_Hundred_Concurrent_Per_Key_Of_Single_Type_With_Key_And_No_Logger() =>
        await ConcurrentTasksWithKeyLoad(ServiceFactoryNoLogger, 1_000, 100);

    [Benchmark]
    public async Task One_Thousand_Tasks_With_One_Hundred_Concurrent_Per_Key_Of_Single_Type_With_ItemKey_And_No_Logger() =>
        await ConcurrentTasksWithItemKeyLoad(ServiceFactoryNoLogger, 1_000, 100);

    [Benchmark]
    public async Task One_Thousand_Tasks_With_One_Hundred_Concurrent_Per_Key_Of_Single_Type_With_Key_And_ILogger() =>
        await ConcurrentTasksWithKeyLoad(ServiceFactoryILogger, 1_000, 100);

    [Benchmark]
    public async Task One_Thousand_Tasks_With_One_Hundred_Concurrent_Per_Key_Of_Single_Type_With_ItemKey_And_ILogger() =>
        await ConcurrentTasksWithItemKeyLoad(ServiceFactoryILogger, 1_000, 100);
}
