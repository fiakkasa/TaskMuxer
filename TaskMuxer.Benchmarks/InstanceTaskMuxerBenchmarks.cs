namespace TaskMuxer.Benchmarks;

[MemoryDiagnoser(true)]
[ThreadingDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[BenchmarkCategory(new[] { nameof(InstanceTaskMultiplexer), "SingleType", "Polymorphic" })]
[WarmupCount(2)]
[IterationCount(3)]
[RPlotExporter]
public class InstanceTaskMultiplexerBenchmarks
{
    public static InstanceTaskMultiplexer ServiceFactoryNoLogger => new();
    public static InstanceTaskMultiplexer ServiceFactoryILogger => new(logger: Substitute.For<ILogger<InstanceTaskMultiplexer>>());

    public static Func<CancellationToken, Task<int>> GetSingleTypeCompletedWorkFuncFactory() =>
        _ => Task.FromResult(1);

    public static Func<CancellationToken, Task<int>> GetSingleTypeWithSimulatedWorkFuncFactory(int delay = 500) =>
        ct => Task.Run(async () =>
        {
            await Task.Delay(delay, ct);
            return 1;
        }, ct);

    public static Func<CancellationToken, Task<object?>> GetPolymorphicTypeCompletedWorkFuncFactory(int kind = 0) =>
        _ => Task.FromResult<object?>((kind % 5) switch
        {
            0 => 1,
            1 => "hello",
            2 => true,
            3 => (1, "hello"),
            _ => new { Test = 1 }
        }
    );

    public static Func<CancellationToken, Task<object?>> GetPolymorphicTypeWithSimulatedWorkFuncFactory(int kind = 0, int delay = 500) =>
        ct => Task.Run<object?>(async () =>
        {
            await Task.Delay(delay, ct);
            return (kind % 5) switch
            {
                0 => 1,
                1 => "hello",
                2 => true,
                3 => (1, "hello"),
                _ => new { Test = 1 }
            };
        }, ct);

    [Benchmark]
    public async Task Single_Completed_Task_Of_Single_Type_With_Key_And_No_Logger()
    {
        await ServiceFactoryNoLogger.AddTask("test", GetSingleTypeCompletedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Completed_Task_Of_Single_Type_With_ItemKey_And_No_Logger()
    {
        await ServiceFactoryNoLogger.AddTask(new ItemKey("test", typeof(int)), GetSingleTypeCompletedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Completed_Task_Of_Single_Type_With_Key_And_ILogger()
    {
        await ServiceFactoryILogger.AddTask("test", GetSingleTypeCompletedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Completed_Task_Of_Single_Type_With_ItemKey_And_ILogger()
    {
        await ServiceFactoryILogger.AddTask(new ItemKey("test", typeof(int)), GetSingleTypeCompletedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Task_Of_Single_Type_With_Key_And_No_Logger()
    {
        await ServiceFactoryNoLogger.AddTask("test", GetSingleTypeWithSimulatedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Task_Of_Single_Type_With_ItemKey_And_No_Logger()
    {
        await ServiceFactoryNoLogger.AddTask(new ItemKey("test", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Task_Of_Single_Type_With_Key_And_ILogger()
    {
        await ServiceFactoryILogger.AddTask("test", GetSingleTypeWithSimulatedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Task_Of_Single_Type_With_ItemKey_And_ILogger()
    {
        await ServiceFactoryILogger.AddTask(new ItemKey("test", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Five_Different_Completed_Tasks_Of_Single_Type_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            service.AddTask("test1", GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask("test2", GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask("test3", GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask("test4", GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask("test5", GetSingleTypeCompletedWorkFuncFactory())
        );
    }

    [Benchmark]
    public async Task Five_Different_Completed_Tasks_Of_Single_Type_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            service.AddTask(new ItemKey("test1", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask(new ItemKey("test2", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask(new ItemKey("test3", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask(new ItemKey("test4", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask(new ItemKey("test5", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
        );
    }

    [Benchmark]
    public async Task Five_Different_Completed_Tasks_Of_Single_Type_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            service.AddTask("test1", GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask("test2", GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask("test3", GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask("test4", GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask("test5", GetSingleTypeCompletedWorkFuncFactory())
        );
    }

    [Benchmark]
    public async Task Five_Different_Completed_Tasks_Of_Single_Type_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            service.AddTask(new ItemKey("test1", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask(new ItemKey("test2", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask(new ItemKey("test3", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask(new ItemKey("test4", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
            , service.AddTask(new ItemKey("test5", typeof(int)), GetSingleTypeCompletedWorkFuncFactory())
        );
    }

    [Benchmark]
    public async Task Five_Different_Tasks_Of_Single_Type_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            service.AddTask("test1", GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask("test2", GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask("test3", GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask("test4", GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask("test5", GetSingleTypeWithSimulatedWorkFuncFactory())
        );
    }

    [Benchmark]
    public async Task Five_Different_Tasks_Of_Single_Type_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            service.AddTask(new ItemKey("test1", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask(new ItemKey("test2", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask(new ItemKey("test3", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask(new ItemKey("test4", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask(new ItemKey("test5", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
        );
    }

    [Benchmark]
    public async Task Five_Different_Tasks_Of_Single_Type_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            service.AddTask("test1", GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask("test2", GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask("test3", GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask("test4", GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask("test5", GetSingleTypeWithSimulatedWorkFuncFactory())
        );
    }

    [Benchmark]
    public async Task Five_Different_Tasks_Of_Single_Type_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            service.AddTask(new ItemKey("test1", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask(new ItemKey("test2", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask(new ItemKey("test3", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask(new ItemKey("test4", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
            , service.AddTask(new ItemKey("test5", typeof(int)), GetSingleTypeWithSimulatedWorkFuncFactory())
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Tasks_Of_Single_Type_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + x,
                    GetSingleTypeCompletedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Tasks_Of_Single_Type_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + x, typeof(int)),
                    GetSingleTypeCompletedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Tasks_Of_Single_Type_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + x,
                    GetSingleTypeCompletedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Tasks_Of_Single_Type_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + x, typeof(int)),
                    GetSingleTypeCompletedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Tasks_Of_Single_Type_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + x,
                    GetSingleTypeWithSimulatedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Tasks_Of_Single_Type_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + x, typeof(int)),
                    GetSingleTypeWithSimulatedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Tasks_Of_Single_Type_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + x,
                    GetSingleTypeWithSimulatedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Tasks_Of_Single_Type_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + x, typeof(int)),
                    GetSingleTypeWithSimulatedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Single_Type_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + (x % 10),
                    GetSingleTypeWithSimulatedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Single_Type_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + (x % 10), typeof(int)),
                    GetSingleTypeWithSimulatedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Single_Type_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + (x % 10),
                    GetSingleTypeWithSimulatedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Single_Type_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + (x % 10), typeof(int)),
                    GetSingleTypeWithSimulatedWorkFuncFactory()
                )
            )
        );
    }

    [Benchmark]
    public async Task Single_Completed_Task_Of_Polymorphic_Types_With_Key_And_No_Logger()
    {
        await ServiceFactoryNoLogger.AddTask("test", GetPolymorphicTypeCompletedWorkFuncFactory(0));
    }

    [Benchmark]
    public async Task Single_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger()
    {
        await ServiceFactoryNoLogger.AddTask(new ItemKey("test", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Completed_Task_Of_Polymorphic_Types_With_Key_And_ILogger()
    {
        await ServiceFactoryILogger.AddTask("test", GetPolymorphicTypeCompletedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Completed_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger()
    {
        await ServiceFactoryILogger.AddTask(new ItemKey("test", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Task_Of_Polymorphic_Types_With_Key_And_No_Logger()
    {
        await ServiceFactoryNoLogger.AddTask("test", GetPolymorphicTypeWithSimulatedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Task_Of_Polymorphic_Types_With_ItemKey_And_No_Logger()
    {
        await ServiceFactoryNoLogger.AddTask(new ItemKey("test", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Task_Of_Polymorphic_Types_With_Key_And_ILogger()
    {
        await ServiceFactoryILogger.AddTask("test", GetPolymorphicTypeWithSimulatedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Single_Task_Of_Polymorphic_Types_With_ItemKey_And_ILogger()
    {
        await ServiceFactoryILogger.AddTask(new ItemKey("test", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory());
    }

    [Benchmark]
    public async Task Five_Different_Completed_Tasks_Of_Polymorphic_Types_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            service.AddTask("test1", GetPolymorphicTypeCompletedWorkFuncFactory(0))
            , service.AddTask("test2", GetPolymorphicTypeCompletedWorkFuncFactory(1))
            , service.AddTask("test3", GetPolymorphicTypeCompletedWorkFuncFactory(2))
            , service.AddTask("test4", GetPolymorphicTypeCompletedWorkFuncFactory(3))
            , service.AddTask("test5", GetPolymorphicTypeCompletedWorkFuncFactory(4))
        );
    }

    [Benchmark]
    public async Task Five_Different_Completed_Tasks_Of_Polymorphic_Types_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            service.AddTask(new ItemKey("test1", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(0))
            , service.AddTask(new ItemKey("test2", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(1))
            , service.AddTask(new ItemKey("test3", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(2))
            , service.AddTask(new ItemKey("test4", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(3))
            , service.AddTask(new ItemKey("test5", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(4))
        );
    }

    [Benchmark]
    public async Task Five_Different_Completed_Tasks_Of_Polymorphic_Types_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            service.AddTask("test1", GetPolymorphicTypeCompletedWorkFuncFactory(0))
            , service.AddTask("test2", GetPolymorphicTypeCompletedWorkFuncFactory(1))
            , service.AddTask("test3", GetPolymorphicTypeCompletedWorkFuncFactory(2))
            , service.AddTask("test4", GetPolymorphicTypeCompletedWorkFuncFactory(3))
            , service.AddTask("test5", GetPolymorphicTypeCompletedWorkFuncFactory(4))
        );
    }

    [Benchmark]
    public async Task Five_Different_Completed_Tasks_Of_Polymorphic_Types_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            service.AddTask(new ItemKey("test1", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(0))
            , service.AddTask(new ItemKey("test2", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(1))
            , service.AddTask(new ItemKey("test3", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(2))
            , service.AddTask(new ItemKey("test4", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(3))
            , service.AddTask(new ItemKey("test5", typeof(int)), GetPolymorphicTypeCompletedWorkFuncFactory(4))
        );
    }

    [Benchmark]
    public async Task Five_Different_Tasks_Of_Polymorphic_Types_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            service.AddTask("test1", GetPolymorphicTypeWithSimulatedWorkFuncFactory(0))
            , service.AddTask("test2", GetPolymorphicTypeWithSimulatedWorkFuncFactory(1))
            , service.AddTask("test3", GetPolymorphicTypeWithSimulatedWorkFuncFactory(2))
            , service.AddTask("test4", GetPolymorphicTypeWithSimulatedWorkFuncFactory(3))
            , service.AddTask("test5", GetPolymorphicTypeWithSimulatedWorkFuncFactory(4))
        );
    }

    [Benchmark]
    public async Task Five_Different_Tasks_Of_Polymorphic_Types_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            service.AddTask(new ItemKey("test1", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(0))
            , service.AddTask(new ItemKey("test2", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(1))
            , service.AddTask(new ItemKey("test3", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(2))
            , service.AddTask(new ItemKey("test4", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(3))
            , service.AddTask(new ItemKey("test5", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(4))
        );
    }

    [Benchmark]
    public async Task Five_Different_Tasks_Of_Polymorphic_Types_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            service.AddTask("test1", GetPolymorphicTypeWithSimulatedWorkFuncFactory(0))
            , service.AddTask("test2", GetPolymorphicTypeWithSimulatedWorkFuncFactory(1))
            , service.AddTask("test3", GetPolymorphicTypeWithSimulatedWorkFuncFactory(2))
            , service.AddTask("test4", GetPolymorphicTypeWithSimulatedWorkFuncFactory(3))
            , service.AddTask("test5", GetPolymorphicTypeWithSimulatedWorkFuncFactory(4))
        );
    }

    [Benchmark]
    public async Task Five_Different_Tasks_Of_Polymorphic_Types_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            service.AddTask(new ItemKey("test1", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(0))
            , service.AddTask(new ItemKey("test2", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(1))
            , service.AddTask(new ItemKey("test3", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(2))
            , service.AddTask(new ItemKey("test4", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(3))
            , service.AddTask(new ItemKey("test5", typeof(int)), GetPolymorphicTypeWithSimulatedWorkFuncFactory(4))
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Tasks_Of_Polymorphic_Types_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + x,
                    GetPolymorphicTypeCompletedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Tasks_Of_Polymorphic_Types_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + x, typeof(int)),
                    GetPolymorphicTypeCompletedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Tasks_Of_Polymorphic_Types_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + x,
                    GetPolymorphicTypeCompletedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Completed_Tasks_Of_Polymorphic_Types_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + x, typeof(int)),
                    GetPolymorphicTypeCompletedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Tasks_Of_Polymorphic_Types_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + x,
                    GetPolymorphicTypeWithSimulatedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Tasks_Of_Polymorphic_Types_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + x, typeof(int)),
                    GetPolymorphicTypeWithSimulatedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Tasks_Of_Polymorphic_Types_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + x,
                    GetPolymorphicTypeWithSimulatedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Different_Tasks_Of_Polymorphic_Types_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + x, typeof(int)),
                    GetPolymorphicTypeWithSimulatedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Polymorphic_Types_With_Key_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + (x % 10),
                    GetPolymorphicTypeWithSimulatedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Polymorphic_Types_With_ItemKey_And_No_Logger()
    {
        var service = ServiceFactoryNoLogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + (x % 10), typeof(int)),
                    GetPolymorphicTypeWithSimulatedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Polymorphic_Types_With_Key_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    "test" + (x % 10),
                    GetPolymorphicTypeWithSimulatedWorkFuncFactory(x)
                )
            )
        );
    }

    [Benchmark]
    public async Task One_Hundred_Tasks_With_Ten_Concurrent_Per_Key_Of_Polymorphic_Types_With_ItemKey_And_ILogger()
    {
        var service = ServiceFactoryILogger;
        await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(x =>
                service.AddTask(
                    new ItemKey("test" + (x % 10), typeof(int)),
                    GetPolymorphicTypeWithSimulatedWorkFuncFactory(x)
                )
            )
        );
    }
}

