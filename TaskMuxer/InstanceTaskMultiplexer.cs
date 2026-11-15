using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace TaskMuxer;

public sealed class InstanceTaskMultiplexer : ITaskMultiplexer, IDisposable
{
    private const string KEY = "Key";
    private const string COUNT = "Count";
    private const string TIMESTAMP = "Timestamp";
    private const string MESSAGE = "Message";
    private const string ITEM_ADDED_MESSAGE =
        $$"""
        Request with key {{{KEY}}} was added to the items list
        Number of items in list: {{{COUNT}}}
        """;
    private const string ITEM_REMOVED_MESSAGE =
        $$"""
        Request with key {{{KEY}}} was removed from the items list
        Number of items remaining in the list: {{{COUNT}}}
        """;
    private const string ITEM_ALREADY_PRESENT_MESSAGE =
        $$"""Request with key {{{KEY}}} is already present in the items list and the existing instance will be returned instead""";
    private const string ITEM_NOT_ADDED =
        $$"""Request with key {{{KEY}}} was not added in the items list""";
    private const string ITEM_STARTING_MESSAGE =
        $$"""Request with key {{{KEY}}} is starting at {{{TIMESTAMP}}}""";
    private const string ITEM_CANCELLED_MESSAGE =
        $$"""Request with key {{{KEY}}}, was cancelled by {CancellationSource} cancellation source, with message: {{{MESSAGE}}}""";
    private const string ITEM_FAILED_MESSAGE =
        $$"""Request with key {{{KEY}}} failed with message: {{{MESSAGE}}}""";
    private const string ITEM_COMPLETED_MESSAGE =
        $$"""Request with key {{{KEY}}} has completed at {{{TIMESTAMP}}}, after {TimeElapsed}, {ResultSuccessStatus}, and will be removed from the items list""";
    private const string ITEM_PRESERVE_MESSAGE =
        $$"""Request with key {{{KEY}}} will be preserved for {PreserveExecutionResultDuration}""";

    private const int _evictionDelay = 250;
    private const int _defaultCollectionCapacity = 100;
    private const int _defaultConcurrency = 1;

    private readonly SemaphoreSlim _addSemaphore = new(_defaultConcurrency);
    private readonly InstanceTaskMultiplexerConfig? _config;
    private readonly ConcurrentDictionary<ItemKey, InstanceItem> _items;
    private readonly ILogger<InstanceTaskMultiplexer>? _logger;
    private bool _serviceable = true;

    public InstanceTaskMultiplexer(InstanceTaskMultiplexerConfig? config = default, ILogger<InstanceTaskMultiplexer>? logger = default)
    {
        (_config, _logger, _items) =
        (
            config,
            logger,
            new ConcurrentDictionary<ItemKey, InstanceItem>(
                _defaultConcurrency,
                config switch
                {
                    { CollectionCapacity: >= 10 or <= 10_000 } c => c.CollectionCapacity,
                    _ => _defaultCollectionCapacity
                }
            )
        );
    }

    private ConcurrentDictionary<ItemKey, InstanceItem> Items
    {
        get
        {
            EnsureServiceable();
            return _items;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task<long> ItemsCount(CancellationToken cancellationToken = default) =>
        Task.FromResult((long)Items.Count);

    public Task<ICollection<ItemKey>> ItemKeys(CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Keys);

    public Task<ItemStatus> GetTaskStatus<T>(string key, CancellationToken cancellationToken = default) =>
        GetTaskStatus(GenerateKey<T>(key), cancellationToken);

    public Task<ItemStatus> GetTaskStatus(ItemKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(
            GetItem(key) switch
            {
                { } item => item.Status,
                _ => default
            }
        );

    public Task<bool> HasTask<T>(string key, CancellationToken cancellationToken = default) =>
        HasTask(GenerateKey<T>(key), cancellationToken);

    public Task<bool> HasTask(ItemKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.ContainsKey(key));

    public Task<Task<T?>?> GetTask<T>(string key, CancellationToken cancellationToken = default) =>
        GetTask<T>(GenerateKey<T>(key), cancellationToken);

    public Task<Task<T?>?> GetTask<T>(ItemKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(GetItemTask<T>(key));

    public Task<bool> CancelTask<T>(string key, bool waitForEviction = false, CancellationToken cancellationToken = default) =>
        CancelTask<T>(GenerateKey<T>(key), waitForEviction, cancellationToken);

    public async Task<bool> CancelTask<T>(ItemKey key, bool waitForEviction = false, CancellationToken cancellationToken = default)
    {
        if (GetItem(key) is not { } item) return false;

        CancelOperation(item.InternalCancellationTokenSource, item.PreserveExecutionResultCancellationTokenSource);

        if (!waitForEviction) return true;

        while (GetItem(key) is not null) await Task.Delay(_evictionDelay, cancellationToken);

        return true;
    }

    public Task<T?> AddTask<T>(string key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default) =>
        AddTask(GenerateKey<T>(key), func, cancellationToken);

    public Task<T?> AddTask<T>(ItemKey key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default) =>
        AddTask(key, func, false, cancellationToken);

    public Task<T?> AddLongRunningTask<T>(
        string key,
        Func<CancellationToken, Task<T?>> func,
        CancellationToken cancellationToken = default
    ) =>
        AddLongRunningTask(GenerateKey<T>(key), func, cancellationToken);

    public Task<T?> AddLongRunningTask<T>(
        ItemKey key,
        Func<CancellationToken, Task<T?>> func,
        CancellationToken cancellationToken = default
    ) =>
        AddTask(key, func, true, cancellationToken);

    private void EnsureServiceable()
    {
        if (!_serviceable) throw new ObjectDisposedException(nameof(InstanceTaskMultiplexer));
    }

    private static ItemKey GenerateKey<T>(string key) => new(key, typeof(T));

    private static void CancelOperation(
        CancellationTokenSource internalCancellationTokenSource,
        CancellationTokenSource preserveExecutionResultCancellationTokenSource
    )
    {
        internalCancellationTokenSource.Cancel();
        preserveExecutionResultCancellationTokenSource.Cancel();
    }

    private static void DisposeOperation<T>(
        TaskCompletionSource<T?> taskCompletionSource,
        CancellationTokenSource internalCancellationTokenSource,
        CancellationTokenSource linkedCancellationTokenSource,
        CancellationTokenSource preserveExecutionResultCancellationTokenSource
    )
    {
        linkedCancellationTokenSource.Dispose();
        internalCancellationTokenSource.Dispose();
        taskCompletionSource.Task.Dispose();
        preserveExecutionResultCancellationTokenSource.Dispose();
    }

    private CancellationTokenSource GenerateInternalCancellationTokenSource(bool longRunning) =>
        (longRunning, _config) switch
        {
            (false, { ExecutionTimeout: { } ExecutionTimeout }) when ExecutionTimeout > TimeSpan.Zero =>
                new CancellationTokenSource(ExecutionTimeout),
            (true, { ExecutionTimeout: { } ExecutionTimeout, LongRunningTaskExecutionTimeout: { } LongRunningTaskExecutionTimeout })
                when ExecutionTimeout > TimeSpan.Zero && LongRunningTaskExecutionTimeout >= ExecutionTimeout =>
                new CancellationTokenSource(LongRunningTaskExecutionTimeout),
            _ => new CancellationTokenSource()
        };

    private InstanceItem? GetItem(ItemKey key) => Items.TryGetValue(key, out var item) switch
    {
        true => item,
        _ => default
    };

    private Task<T?>? GetItemTask<T>(ItemKey key) => GetItem(key) switch
    {
        { } item when item.TaskCompletionSource is TaskCompletionSource<T?> itemValue => itemValue.Task,
        _ => default
    };

    private async Task<T?> AddTask<T>(
        ItemKey key,
        Func<CancellationToken, Task<T?>> func,
        bool longRunning,
        CancellationToken cancellationToken = default
    )
    {
        EnsureServiceable();

#pragma warning disable S6966
        _addSemaphore.Wait(CancellationToken.None);
#pragma warning restore

        if (GetItemTask<T>(key) is { } taskInstance)
        {
            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger.LogInformation(ITEM_ALREADY_PRESENT_MESSAGE, key);
            }

            _addSemaphore.Release();

            return await taskInstance;
        }

        var taskCompletionSource = new TaskCompletionSource<T?>();
        var internalCancellationTokenSource = GenerateInternalCancellationTokenSource(longRunning);
        var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            internalCancellationTokenSource.Token,
            cancellationToken
        );
        var preserveExecutionResultDuration = _config?.PreserveExecutionResultDuration ?? TimeSpan.Zero;
        var preserveExecutionResultCancellationTokenSource = new CancellationTokenSource();
        var item = new InstanceItem<T>
        {
            TaskCompletionSource = taskCompletionSource,
            InternalCancellationTokenSource = internalCancellationTokenSource,
            PreserveExecutionResultCancellationTokenSource = preserveExecutionResultCancellationTokenSource,
            Status = ItemStatus.Created
        };

        if (!Items.TryAdd(key, item))
        {
            if (_logger?.IsEnabled(LogLevel.Warning) == true)
            {
                _logger.LogWarning(ITEM_NOT_ADDED, key);
            }

            DisposeOperation(
                taskCompletionSource,
                internalCancellationTokenSource,
                linkedCancellationTokenSource,
                preserveExecutionResultCancellationTokenSource
            );

            _addSemaphore.Release();

            return default;
        }

        item.Status = ItemStatus.Added;

        if (_logger?.IsEnabled(LogLevel.Information) == true)
        {
            _logger.LogInformation(ITEM_ADDED_MESSAGE, key, Items.Count);
        }

        _addSemaphore.Release();

        RunItemWorkflow(
            key,
            item,
            taskCompletionSource,
            func,
            internalCancellationTokenSource,
            linkedCancellationTokenSource,
            preserveExecutionResultDuration,
            preserveExecutionResultCancellationTokenSource
        );

        return await taskCompletionSource.Task;
    }

#pragma warning disable S107, S3776
    private void RunItemWorkflow<T>(
        ItemKey key,
        InstanceItem<T> item,
        TaskCompletionSource<T?> taskCompletionSource,
        Func<CancellationToken, Task<T?>> func,
        CancellationTokenSource internalCancellationTokenSource,
        CancellationTokenSource linkedCancellationTokenSource,
        TimeSpan preserveExecutionResultDuration,
        CancellationTokenSource preserveExecutionResultCancellationTokenSource
    )
#pragma warning restore
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        item.Status = ItemStatus.Starting;

        if (_logger?.IsEnabled(LogLevel.Information) == true)
        {
            _logger.LogInformation(ITEM_STARTING_MESSAGE, key, DateTimeOffset.UtcNow);
        }

        Task.Run(async () =>
            {
                try
                {
                    item.Status = ItemStatus.Started;
                    taskCompletionSource.SetResult(await func(linkedCancellationTokenSource.Token));
                    item.Status = ItemStatus.Completed;
                }
                catch (TaskCanceledException ex)
                {
                    item.Status = ItemStatus.Canceled;

                    if (_logger?.IsEnabled(LogLevel.Warning) == true)
                    {
                        _logger.LogWarning(
                           ex,
                           ITEM_CANCELLED_MESSAGE,
                           key,
                           internalCancellationTokenSource.Token.IsCancellationRequested switch
                           {
                               true => "internal",
                               _ => "external"
                           },
                           ex.Message
                       );
                    }

                    taskCompletionSource.SetCanceled(linkedCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    item.Status = ItemStatus.Failed;

                    if (_logger?.IsEnabled(LogLevel.Error) == true)
                    {
                        _logger.LogError(ex, ITEM_FAILED_MESSAGE, key, ex.Message);
                    }

                    taskCompletionSource.SetException(ex);
                }

                if (_logger?.IsEnabled(LogLevel.Information) == true)
                {
                    _logger.LogInformation(
                       ITEM_COMPLETED_MESSAGE,
                       key,
                       DateTimeOffset.UtcNow,
                       Stopwatch.GetElapsedTime(startTimestamp),
                       item.Status switch
                       {
                           ItemStatus.Completed => "successfully",
                           _ => "unsuccessfully"
                       }
                   );
                }

                if (item.Status == ItemStatus.Completed && preserveExecutionResultDuration > TimeSpan.Zero)
                {
                    if (_logger?.IsEnabled(LogLevel.Information) == true)
                    {
                        _logger.LogInformation(ITEM_PRESERVE_MESSAGE, key, preserveExecutionResultDuration);
                    }

                    try
                    {
                        await Task.Delay(preserveExecutionResultDuration, preserveExecutionResultCancellationTokenSource.Token);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                Items.TryRemove(key, out _);

                if (_logger?.IsEnabled(LogLevel.Information) == true)
                {
                    _logger.LogInformation(ITEM_REMOVED_MESSAGE, key, Items.Count);
                }

                DisposeOperation(
                    taskCompletionSource,
                    internalCancellationTokenSource,
                    linkedCancellationTokenSource,
                    preserveExecutionResultCancellationTokenSource
                );
            },
            internalCancellationTokenSource.Token);
    }

#pragma warning disable S1172
    private void Dispose(bool _)
#pragma warning restore
    {
        if (!_serviceable) return;

        _serviceable = false;

        foreach (var item in _items.Values)
        {
            CancelOperation(item.InternalCancellationTokenSource, item.PreserveExecutionResultCancellationTokenSource);
        }

        _items.Clear();
        _addSemaphore.Dispose();
    }

    ~InstanceTaskMultiplexer()
    {
        Dispose(false);
    }
}
