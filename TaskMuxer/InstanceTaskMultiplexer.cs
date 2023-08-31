using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskMuxer;

public class InstanceTaskMultiplexer : ITaskMultiplexer
{
    private readonly ConcurrentDictionary<ItemKey, ItemValue> _items = new();
    private readonly ILogger<InstanceTaskMultiplexer>? _logger;

    public InstanceTaskMultiplexer() { }
    public InstanceTaskMultiplexer(ILogger<InstanceTaskMultiplexer> logger) => 
        _logger = logger;

    private void LogInformation(string? message, params object[] args) => 
        _logger?.LogInformation(message, args);

    private void LogWarning(Exception? exception, string? message, params object[] args) => 
        _logger?.LogWarning(exception, message, args);

    private void LogError(Exception? exception, string? message, params object[] args) => 
        _logger?.LogError(exception, message, args);

    private static ItemKey GenerateKey<T>(string key) => new(key, typeof(T));

    private T? GetItemValue<T>(ItemKey key) => _items.TryGetValue(key, out var item) switch
    {
        true when item.Value is T itemValue => itemValue,
        _ => default
    };

    private ItemStatus GetItemStatus(ItemKey key) => _items.TryGetValue(key, out var item) switch
    {
        true => item.Status,
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

    public Task<T?> AddTask<T>(string key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default) =>
        AddTask(GenerateKey<T>(key), func, cancellationToken);

    public Task<T?> AddTask<T>(ItemKey key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default)
    {
        if (GetItemTask<T>(key) is { } taskInstance)
        {
            LogInformation(
                "Request with key {Key} is already present in the items list and the existing instance will be returned instead", 
                key
            );

            return taskInstance;
        }

        var taskCompletionSource = new TaskCompletionSource<T?>();
        var newItemValue = new ItemValue(taskCompletionSource)
        {
            Status = ItemStatus.Created
        };

        if (!_items.TryAdd(key, newItemValue))
        {
            LogWarning(
                new DataException($"Request with key {key} was not added in the items list"), 
                "Request with key {Key} was not added in the items list", 
                key
            );

            return Task.FromResult(default(T?));
        }

        newItemValue.Status = ItemStatus.Added;

        LogInformation("Request with key {Key} was added to the items list", key);
        LogInformation("Number of items in list: {Count}", _items.Count);

        try
        {
            return taskCompletionSource.Task;
        }
        finally
        {
            var startTimestamp = Stopwatch.GetTimestamp();

            newItemValue.Status = ItemStatus.Starting;
            LogInformation("Request with key {Key} is starting at {Timestamp}", key, DateTimeOffset.UtcNow);

            Task.Run(async () =>
            {
                newItemValue.Status = ItemStatus.Started;

                try
                {
                    taskCompletionSource.SetResult(await func(cancellationToken));
                    newItemValue.Status = ItemStatus.Completed;
                }
                catch (TaskCanceledException ex)
                {
                    newItemValue.Status = ItemStatus.Canceled;
                    taskCompletionSource.SetCanceled(cancellationToken);
                    LogWarning(ex, "Request with key {Key}, was cancelled with message: {Message}", key, ex.Message);
                }
                catch (Exception ex)
                {
                    newItemValue.Status = ItemStatus.Failed;
                    LogError(ex, "Request with key {Key}, failed with message: {Message}", key, ex.Message);
                    taskCompletionSource.SetException(ex);
                }

                _items.TryRemove(key, out var _);

                LogInformation(
                    "Request with key {Key} has completed at {Timestamp}, after {TimeElapsed}, {ResultMessage}, and will be removed from the items list",
                    key,
                    DateTimeOffset.UtcNow,
                    Stopwatch.GetElapsedTime(startTimestamp),
                    newItemValue.Status switch
                    {
                        ItemStatus.Completed => "successfully",
                        _ => "unsuccessfully"
                    }
                );

                LogInformation("Number of items remaining in the list: {Count}", _items.Count);
            }, cancellationToken);
        }
    }
}
