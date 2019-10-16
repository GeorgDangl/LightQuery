using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LightQuery.Shared.Tests.Regression
{
    public class InheritedPropertiesSortWithSqlite
    {
        public class UserBase
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? FavoriteRestaurantId { get; set; }
            public Restaurant FavoriteRestaurant { get; set; }
        }

        public class User : UserBase { }

        public class Restaurant
        {
            public int Id { get; set; }
            public string Street { get; set; }
        }

        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions options) : base(options)
            {
            }

            public DbSet<UserBase> Users { get; set; }
        }

        private static Dictionary<string, SqliteConnection> _connections = new Dictionary<string, SqliteConnection>();

        private static async Task<AppDbContext> GetContextAsync()
        {
            var connectionId = Guid.NewGuid().ToString();
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = $"in-memory-{connectionId}.db",
                Cache = SqliteCacheMode.Shared,
                Mode = SqliteOpenMode.Memory
            }.ToString();
            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            _connections.Add(connectionId, connection);

            var serviceProvider = new ServiceCollection()
                .AddDbContext<AppDbContext>(options => options
                .UseSqlite(connectionString))
                .BuildServiceProvider();
            using (var setupContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>())
            {
                await setupContext.Database.EnsureCreatedAsync();
                setupContext.Users.Add(new UserBase { Name = "George", FavoriteRestaurant = new Restaurant { Street = "Main Street" } });
                setupContext.Users.Add(new UserBase { Name = "Steve", FavoriteRestaurant = new Restaurant { Street = "Forest Street" } });
                setupContext.Users.Add(new UserBase { Name = "Bob" });
                await setupContext.SaveChangesAsync();
            }

            return serviceProvider.GetRequiredService<AppDbContext>();
        }

        [Fact]
        public async Task CanSortOnPropertyFromBaseClassWhenProjectedToDerivedClass()
        {
            var context = await GetContextAsync();

            var usersQueryable = context.Users
                .Select(u => new User
                {
                    Name = u.Name
                });

            var queryOptions = new QueryOptions
            {
                SortPropertyName = nameof(User.Name)
            };

            var orderedQuery = QueryableProcessor.ApplySorting(usersQueryable, queryOptions);

            dynamic dynamicOrderedQuery = orderedQuery;

            var count = Queryable.Count(dynamicOrderedQuery);

            Assert.Equal(3, count);
        }

        [Fact]
        public async Task CanSortOnNestedProperty()
        {
            // This test ensures that relational sorting works with SQLite
            var context = await GetContextAsync();

            var usersQueryable = context.Users;
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "FavoriteRestaurant.Street"
            };

            var orderedQuery = QueryableProcessor.ApplySorting(usersQueryable, queryOptions);

            dynamic dynamicOrderedQuery = orderedQuery;

            var count = Queryable.Count(dynamicOrderedQuery);

            // It should only have two results since the added null checks remove
            // the user with the missing FavoriteRestaurant
            Assert.Equal(2, count);
        }
    }
}
