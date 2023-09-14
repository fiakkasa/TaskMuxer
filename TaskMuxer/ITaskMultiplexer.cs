namespace TaskMuxer;

public interface ITaskMultiplexer
{
    Task<long> ItemsCount(CancellationToken cancellationToken = default);

    Task<ICollection<ItemKey>> ItemKeys(CancellationToken cancellationToken = default);

    Task<ItemStatus> GetTaskStatus<T>(string key, CancellationToken cancellationToken = default);

    Task<ItemStatus> GetTaskStatus(ItemKey key, CancellationToken cancellationToken = default);

    Task<bool> HasTask<T>(string key, CancellationToken cancellationToken = default);

    Task<bool> HasTask(ItemKey key, CancellationToken cancellationToken = default);

    Task<Task<T?>?> GetTask<T>(string key, CancellationToken cancellationToken = default);

    Task<Task<T?>?> GetTask<T>(ItemKey key, CancellationToken cancellationToken = default);

    Task<bool> CancelTask<T>(string key, bool waitForEviction = false, CancellationToken cancellationToken = default);

    Task<bool> CancelTask<T>(ItemKey key, bool waitForEviction = false, CancellationToken cancellationToken = default);

    Task<T?> AddTask<T>(string key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default);

    Task<T?> AddTask<T>(ItemKey key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default);

    Task<T?> AddLongRunningTask<T>(string key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default);

    Task<T?> AddLongRunningTask<T>(ItemKey key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default);
}