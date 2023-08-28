using Microsoft.EntityFrameworkCore;

namespace TaskMuxer.Tests;

public static partial class EFTestingUtils
{
    public static SampleDataContext DbCtx => new(
        new DbContextOptionsBuilder<SampleDataContext>()
            .UseInMemoryDatabase($"db_{DateTimeOffset.Now.Ticks}_{Random.Shared.NextInt64(999, 99999)}")
            .Options
    );

    public static SampleDataContext GetDbCtx(object[]? collection = default) =>
        GetDbCtx(out var _, collection);

    public static SampleDataContext GetDbCtx(
        out SampleDataContext dbCtx,
        object[]? collection = default
    )
    {
        dbCtx = DbCtx;

        if (collection is { Length: > 0 })
        {
            foreach (var item in collection) dbCtx.Add(item);

            dbCtx.SaveChanges();
        }

        return dbCtx;
    }
}
