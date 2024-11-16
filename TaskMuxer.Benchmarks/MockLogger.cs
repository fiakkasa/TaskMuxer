namespace TaskMuxer.Benchmarks;

public class MockLogger<T> : ILogger<T>, IDisposable
{
    public void Dispose() { }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
