using Microsoft.EntityFrameworkCore;

namespace IQueryable.Filter.Tests
{
    public class TestDbContext : DbContext
    {
        public TestDbContext()
        { }

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        { }

        public DbSet<FilterEntity> FilterEntities { get; set; }
    }
}