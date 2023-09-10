using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskMuxer;

public class InstanceTaskMultiplexer : ITaskMultiplexer
{
    private const int _evictionDelay = 250;
    private const int _defaultCollectionCapacity = 100;
    private const int _defaultConcurrency = 1;
    private readonly ConcurrentDictionary<ItemKey, InstanceItem> _items;
    private readonly ILogger<InstanceTaskMultiplexer>? _logger;
    private readonly InstanceTaskMultiplexerConfig? _config;
    private readonly SemaphoreSlim _addSemaphore = new(_defaultConcurrency);

    public InstanceTaskMultiplexer() =>
        _items = new ConcurrentDictionary<ItemKey, InstanceItem>(
            concurrencyLevel: _defaultConcurrency,
            capacity: _defaultCollectionCapacity
        );

    public InstanceTaskMultiplexer(InstanceTaskMultiplexerConfig? config = default, ILogger<InstanceTaskMultiplexer>? logger = default) =>
        (_config, _logger, _items) =
        (
            config,
            logger,
            new ConcurrentDictionary<ItemKey, InstanceItem>(
                concurrencyLevel: _defaultConcurrency,
                capacity: config switch
                {
                    { CollectionCapacity: >= 10 or <= 10_000 } c => c.CollectionCapacity,
                    _ => _defaultCollectionCapacity
                }
            )
        );

#pragma warning disable CA2254
    private void LogInformation(string? message, params object[] args) =>
        _logger?.LogInformation(message, args);

    private void LogWarning(Exception? exception, string? message, params object[] args) =>
        _logger?.LogWarning(exception, message, args);

    private void LogError(Exception? exception, string? message, params object[] args) =>
        _logger?.LogError(exception, message, args);
#pragma warning restore CA2254

    private static ItemKey GenerateKey<T>(string key) => new(key, typeof(T));

    private CancellationTokenSource GenerateInternalCancellationTokenSource() =>
        _config?.ExecutionTimeout switch
        {
            { } ExecutionTimeout when ExecutionTimeout > TimeSpan.Zero => new CancellationTokenSource(ExecutionTimeout),
            _ => new CancellationTokenSource()
        };

    private InstanceItem? GetItem(ItemKey key) => _items.TryGetValue(key, out var item) switch
    {
        true => item,
        _ => default
    };

    private T? GetItemValue<T>(ItemKey key) => GetItem(key) switch
    {
        { } item when item.TaskCompletionSource is T itemValue => itemValue,
        _ => default
    };

    private CancellationTokenSource? GetItemInternalCancellationSource<T>(ItemKey key) => GetItem(key) switch
    {
        { } item => item.InternalCancellationTokenSource,
        _ => default
    };

    private ItemStatus GetItemStatus(ItemKey key) => GetItem(key) switch
    {
        { } item => item.Status,
        _ => default
    };

    private Task<T?>? GetItemTask<T>(ItemKey key) =>
        GetItemValue<TaskCompletionSource<T?>>(key)?.Task;

    public Task<long> ItemsCount(CancellationToken cancellationToken = default) =>
        Task.FromResult((long)_items.Count);

    public Task<ICollection<ItemKey>> ItemKeys(CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.Keys);

    public Task<ItemStatus> GetTaskStatus<T>(string key, CancellationToken cancellationToken = default) =>
        GetTaskStatus(GenerateKey<T>(key), cancellationToken);

    public Task<ItemStatus> GetTaskStatus(ItemKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(GetItemStatus(key));

    public Task<bool> HasTask<T>(string key, CancellationToken cancellationToken = default) =>
        HasTask(GenerateKey<T>(key), cancellationToken);

    public Task<bool> HasTask(ItemKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.ContainsKey(key));

    public Task<Task<T?>?> GetTask<T>(string key, CancellationToken cancellationToken = default) =>
        GetTask<T>(GenerateKey<T>(key), cancellationToken);

    public Task<Task<T?>?> GetTask<T>(ItemKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(GetItemTask<T>(key));

    public Task<bool> CancelTask<T>(string key, bool waitForEviction = false, CancellationToken cancellationToken = default) =>
        CancelTask<T>(GenerateKey<T>(key), waitForEviction, cancellationToken);

    public async Task<bool> CancelTask<T>(ItemKey key, bool waitForEviction = false, CancellationToken cancellationToken = default)
    {
        var cts = GetItemInternalCancellationSource<T>(key);

        if (cts is not { }) return false;

        cts.Cancel();

        while (waitForEviction && GetItem(key) is { })
            await Task.Delay(_evictionDelay, cancellationToken);

        return true;
    }

    public Task<T?> AddTask<T>(string key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default) =>
        AddTask(GenerateKey<T>(key), func, cancellationToken);


    public Task<T?> AddTask<T>(ItemKey key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<T?> taskCompletionSource;
        InstanceItem<T> item;
        CancellationTokenSource internalCancellationTokenSource;
        CancellationTokenSource linkedCancellationTokenSource;

        try
        {
            _addSemaphore.Wait();

            if (GetItemTask<T>(key) is { } taskInstance)
            {
                LogInformation(
                    "Request with key {Key} is already present in the items list and the existing instance will be returned instead",
                    key
                );

                return taskInstance;
            }

            taskCompletionSource = new TaskCompletionSource<T?>();
            internalCancellationTokenSource = GenerateInternalCancellationTokenSource();
            linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(internalCancellationTokenSource.Token, cancellationToken);
            item = new InstanceItem<T>
            {
                TaskCompletionSource = taskCompletionSource,
                InternalCancellationTokenSource = internalCancellationTokenSource,
                Status = ItemStatus.Created
            };

            if (!_items.TryAdd(key, item))
            {
                LogWarning(default, "Request with key {Key} was not added in the items list", key);

                linkedCancellationTokenSource.Dispose();

                return Task.FromResult(default(T?));
            }

            item.Status = ItemStatus.Added;

            LogInformation("Request with key {Key} was added to the items list", key);
            LogInformation("Number of items in list: {Count}", _items.Count);

        }
        finally
        {
            _addSemaphore.Release();
        }

        try
        {
            return taskCompletionSource.Task;
        }
        finally
        {
            RunItemWorkflow(
                key,
                item,
                taskCompletionSource,
                func,
                internalCancellationTokenSource,
                linkedCancellationTokenSource
            );
        }
    }

    private void RunItemWorkflow<T>(
        ItemKey key,
        InstanceItem<T> item,
        TaskCompletionSource<T?> taskCompletionSource,
        Func<CancellationToken, Task<T?>> func,
        CancellationTokenSource internalCancellationTokenSource,
        CancellationTokenSource linkedCancellationTokenSource
    )
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        item.Status = ItemStatus.Starting;
        LogInformation("Request with key {Key} is starting at {Timestamp}", key, DateTimeOffset.UtcNow);

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
                taskCompletionSource.SetCanceled(linkedCancellationTokenSource.Token);
                LogWarning(
                    ex,
                    "Request with key {Key}, was cancelled by {CancellationSource} cancellation source, with message: {Message}",
                    key,
                    internalCancellationTokenSource.Token.IsCancellationRequested switch
                    {
                        true => "internal",
                        _ => "external"
                    },
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                item.Status = ItemStatus.Failed;
                LogError(ex, "Request with key {Key}, failed with message: {Message}", key, ex.Message);
                taskCompletionSource.SetException(ex);
            }

            if (
                item.Status == ItemStatus.Completed
                && _config?.PreserveExecutionResultDuration is { } PreserveExecutionResultDuration
                && PreserveExecutionResultDuration > TimeSpan.Zero
            )
            {
                LogInformation(
                    "Request with key {Key} has completed at {Timestamp}, after {TimeElapsed}, successfully, and will be removed from the items list after {PreserveExecutionResultDuration}",
                    key,
                    DateTimeOffset.UtcNow,
                    Stopwatch.GetElapsedTime(startTimestamp),
                    PreserveExecutionResultDuration
                );
                LogInformation("Number of items remaining in the list: {Count}", _items.Count);

                using var autoResetPreservationTimerEvent = new AutoResetEvent(false);
                using var preservationTimer = new Timer(
                    _ => autoResetPreservationTimerEvent.Set(),
                    null,
                    PreserveExecutionResultDuration,
                    TimeSpan.Zero
                );
                autoResetPreservationTimerEvent.WaitOne();
            }
            else
            {
                LogInformation(
                    "Request with key {Key} has completed at {Timestamp}, after {TimeElapsed}, {ResultSuccessStatus}, and will be removed from the items list",
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

            _items.TryRemove(key, out var _);

            linkedCancellationTokenSource.Dispose();
            internalCancellationTokenSource.Dispose();
            taskCompletionSource.Task.Dispose();

            LogInformation("Request with key {Key} was removed", key);
            LogInformation("Number of items remaining in the list: {Count}", _items.Count);
        }, internalCancellationTokenSource.Token);
    }
}
