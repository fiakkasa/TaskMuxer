using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskMuxer;

public class InstanceTaskMultiplexer : ITaskMultiplexer
{
    private readonly ConcurrentDictionary<ItemKey, ItemValue> _items = new();
    private readonly ILogger<InstanceTaskMultiplexer>? _logger;

    public InstanceTaskMultiplexer() { }
    public InstanceTaskMultiplexer(ILogger<InstanceTaskMultiplexer> logger) => _logger = logger;
    public InstanceTaskMultiplexer(ILoggerFactory loggerFactory) : this(loggerFactory.CreateLogger<InstanceTaskMultiplexer>()) { }

    public Task<long> ItemsCount(CancellationToken cancellationToken = default) => 
        Task.FromResult(_items.LongCount());

    public Task<ICollection<ItemKey>> ItemKeys(CancellationToken cancellationToken = default) => 
        Task.FromResult(_items.Keys);

    private static ItemKey GenerateKey<T>(string key) => new(key, typeof(T));

    public Task<ItemStatus> GetTaskStatus<T>(string key, CancellationToken cancellationToken = default) => 
        GetTaskStatus(GenerateKey<T>(key), cancellationToken);

    public Task<ItemStatus> GetTaskStatus(ItemKey key, CancellationToken cancellationToken = default) => Task.FromResult(
        _items.TryGetValue(key, out var itemValue) switch
        {
            true => itemValue.Status,
            _ => ItemStatus.None
        }
    );

    public Task<T?> AddTask<T>(string key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default)
    {
        var typedKey = GenerateKey<T>(key);
        var taskCompletionSource = new TaskCompletionSource<T?>();
        var newItemValue = new ItemValue(taskCompletionSource)
        {
            Status = ItemStatus.Created
        };

        if (!_items.TryAdd(typedKey, newItemValue))
        {
            _items.TryGetValue(typedKey, out var itemValue);

            var taskInstance = ((TaskCompletionSource<T?>)itemValue!.Value).Task;

            _logger?.LogInformation("Request with key {Key} is already present in the items list and the existing instance will be returned instead", typedKey);

            return taskInstance;
        }

        newItemValue.Status = ItemStatus.Added;

        _logger?.LogInformation("Request with key {Key} was added to the items list", typedKey);
        _logger?.LogInformation("Number of items in list: {Count}", _items.Count);

        try
        {
            return taskCompletionSource.Task;
        }
        finally
        {
            var startTimestamp = Stopwatch.GetTimestamp();

            newItemValue.Status = ItemStatus.Starting;
            _logger?.LogInformation("Request with key {Key} is starting at {Timestamp}", typedKey, DateTimeOffset.UtcNow);

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
                    _logger?.LogWarning(ex, "Request with key {Key}, was cancelled with message: {Message}", typedKey, ex.Message);
                }
                catch (Exception ex)
                {
                    newItemValue.Status = ItemStatus.Failed;
                    _logger?.LogError(ex, "Request with key {Key}, failed with message: {Message}", typedKey, ex.Message);
                    taskCompletionSource.SetException(ex);
                }

                _items.TryRemove(typedKey, out var _);

                _logger?.LogInformation(
                    "Request with key {Key} has completed at {Timestamp}, after {TimeElapsed}, {ResultMessage}, and will be removed from the items list",
                    typedKey,
                    DateTimeOffset.UtcNow,
                    Stopwatch.GetElapsedTime(startTimestamp),
                    newItemValue.Status switch
                    {
                        ItemStatus.Completed => "successfully",
                        _ => "unsuccessfully"
                    }
                );

                _logger?.LogInformation("Number of items remaining in the list: {Count}", _items.Count);
            }, CancellationToken.None);
        }
    }
}
