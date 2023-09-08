using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TaskMuxer.Tests;

public class ServiceTests
{
    public class SampleService
    {
        private readonly ITaskMultiplexer _taskMultiplexer;
        private readonly SampleDataContext _dbContext;

        public IQueryable<SampleEntity> Samples => _dbContext.Samples;

        private readonly SemaphoreSlim _semaphoreSlim = new(1);

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

        public async Task<long> ComputeNextNumber(CancellationToken cancellationToken = default) =>
            await _taskMultiplexer.AddTask(
                "compute_next_number",
                async ct =>
                {
                    await Task.Delay(250, ct);
                    return Random.Shared.Next();
                },
                cancellationToken
            );
    }

    [Fact]
    public async Task Service_Returns_Computed_Results()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddDbContext<SampleDataContext>()
            .AddInstanceTaskMultiplexerWithILogger()
            .AddSingleton<SampleService>()
            .BuildServiceProvider();

        var service = serviceProvider.GetRequiredService<SampleService>();

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select((_, __) => service.ComputeNextNumber()));

        Assert.Equal(10, results.Length);
        Assert.Single(results.Distinct());
    }

    [Fact]
    public async Task Service_GetById_EF()
    {
        var service = new SampleService(
            new InstanceTaskMultiplexer(),
            EFTestingUtils.GetDbCtx(
                Enumerable.Range(1, 10)
                    .Select(v => new SampleEntity
                    {
                        Id = v,
                        Text = "Text " + v
                    })
                    .ToArray()
            )
        );

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select((_, __) => service.GetById(5)));

        Assert.Equal(10, results.Length);
        Assert.Single(results.Distinct());
    }

    [Fact]
    public async Task Service_Add_EF()
    {
        var service = new SampleService(new InstanceTaskMultiplexer(), EFTestingUtils.GetDbCtx());

        var results = await Task.WhenAll(
            Enumerable.Range(0, 10)
                .Select((_, __) => service.Add(
                    new()
                    {
                        Text = "Random " + Random.Shared.Next()
                    })
                )
            );

        Assert.Single(service.Samples);
    }

    [Fact]
    public async Task Service_Mixed_EF()
    {
        var service = new SampleService(new InstanceTaskMultiplexer(), EFTestingUtils.GetDbCtx());

        await Task.WhenAll(
            service.GetById(1),
            service.Add(
                new()
                {
                    Text = "Random " + Random.Shared.Next()
                }
            ),
            service.GetById(1),
            service.Add(
                new()
                {
                    Text = "Random " + Random.Shared.Next()
                }
            )
        );

        Assert.Single(service.Samples);
    }
}