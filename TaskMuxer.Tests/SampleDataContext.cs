using Microsoft.EntityFrameworkCore;

namespace TaskMuxer.Tests;

public class SampleDataContext : DbContext
{
    public SampleDataContext(DbContextOptions<SampleDataContext> options) : base(options) { }

    public DbSet<SampleEntity> Samples { get; set; } = default!;
}
