using Microsoft.EntityFrameworkCore;

namespace LightQuery.Tests.Integration
{
    public class LightQueryContext : DbContext
    {
        public LightQueryContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
