using Microsoft.EntityFrameworkCore;

namespace LightQuery.IntegrationTestsServer
{
    public class LightQueryContext : DbContext
    {
        public LightQueryContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
