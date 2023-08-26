﻿namespace TaskMultiplexer;

public interface ITaskMultiplexer
{
    Task<long> ItemsCount(CancellationToken cancellationToken = default);
    
    Task<ICollection<ItemKey>> ItemKeys(CancellationToken cancellationToken = default);

    Task<ItemStatus> GetTaskStatus<T>(string key, CancellationToken cancellationToken = default);

    Task<T?> AddTask<T>(string key, Func<CancellationToken, Task<T?>> func, CancellationToken cancellationToken = default);
}