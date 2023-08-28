# TaskMuxer

[![NuGet Version](https://img.shields.io/nuget/v/TaskMuxer)](https://www.nuget.org/packages/TaskMuxer)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TaskMuxer)](https://www.nuget.org/packages/TaskMuxer)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/fiakkasa/TaskMuxer/blob/master/LICENSE)

Task Multiplexer.

The purpose of this package is to provide the ability to reduce the amount of parallel repetitive work.

In practical terms, imagine a service that requests data from a slow external API serving multiple requests in parallel.

By using this library, for any number of parallel requests only one will be executed.

Tasks are being identified / segmented by a key and their respective return type.

[Nuget](https://www.nuget.org/packages/TaskMuxer/)

## Usage

### Registration

Locate the services registration and append one of:

- `.AddInstanceTaskMultiplexerNoLogger` - use when no logging is required
- `.AddInstanceTaskMultiplexerWithILogger` - use when the ILogger provider is available
- `.AddInstanceTaskMultiplexerWithILoggerFactory` - use when the ILoggerFactory provider is available

ex.

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // ...
    services
        .AddInstanceTaskMultiplexerWithILogger();
    // ...
}
```

📝 If further customization is required, consider wiring up any of the `ITaskMultiplexer` implementations as required.

### Code

Please check the `ITaskMultiplexer` interface for the API surface provided.

ex.

```csharp
public class ItemsCountService
{
    readonly ITaskMultiplexer _taskMultiplexer;

    public ItemsCountService(ITaskMultiplexer taskMultiplexer) => _taskMultiplexer = taskMultiplexer;

    public async Task<long> GetCountOfItems(CancellationToken cancellationToken = default) =>
        await _taskMultiplexer.AddTask(
            "items_count",
            async ct =>
            {
                await Task.Delay(250, ct);
                return Random.Shared.Next();
            },
            cancellationToken
        );
}
```

### Thread Safety

To achieve thread safety in cases like entity framework core accessing and / or writing data at the same time through parallel threads consider using [semaphores](https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim) or some other form of locking.

ex.

```csharp
public record SampleEntity
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;
}

public class SampleDataContext : DbContext
{
    public SampleDataContext(DbContextOptions<SampleDataContext> options) : base(options) { }

    public DbSet<SampleEntity> Samples { get; set; } = default!;
}

public class SampleService
{
    readonly ITaskMultiplexer _taskMultiplexer;
    readonly SampleDataContext _dbContext;

    private SemaphoreSlim _semaphoreSlim = new(1);

    public SampleService(ITaskMultiplexer taskMultiplexer, SampleDataContext dbContext) =>
        (_taskMultiplexer, _dbContext) = (taskMultiplexer, dbContext);

    public async Task<SampleEntity?> GetById(int id, CancellationToken cancellationToken = default) =>
        await _taskMultiplexer.AddTask(
            "get_by_id_with_semaphore",
            async ct =>
            {
                await _semaphoreSlim.WaitAsync(ct);

                try
                {
                    return await _dbContext.Samples.FirstOrDefaultAsync(x => x.Id == id, ct);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            },
            cancellationToken
        );

    public async Task<SampleEntity?> Add(SampleEntity obj, CancellationToken cancellationToken = default) =>
        await _taskMultiplexer.AddTask(
            "add_with_semaphore",
            async ct =>
            {
                await _semaphoreSlim.WaitAsync(ct);

                try
                {
                    _dbContext.Add(obj);
                    await _dbContext.SaveChangesAsync(ct);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }

                return obj;
            },
            cancellationToken
        );
}

var service = new SampleService(
    new InstanceTaskMultiplexer(),
    new SampleDataContext(
        new DbContextOptionsBuilder<SampleDataContext>()
            .UseInMemoryDatabase("sample_database")
            .Options
    )
);

await Task.WhenAll(
    service.GetById(1),
    service.Add(
        new()
        {
            Text = "Random " + Random.Shared.Next()
        }
    ),
    service.GetById(1)
);
```

## Similar Projects

- https://github.com/lundog/TaskMultiplexer
