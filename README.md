# TaskMuxer

[![NuGet Version](https://img.shields.io/nuget/v/TaskMuxer)](https://www.nuget.org/packages/TaskMuxer)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TaskMuxer)](https://www.nuget.org/packages/TaskMuxer)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/fiakkasa/TaskMuxer/blob/master/LICENSE)

Task Multiplexer.

The purpose of this package is to provide the ability to reduce the amount of parallel repetitive work in a thread safe manner.

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

## Similar Projects

- https://github.com/lundog/TaskMultiplexer
