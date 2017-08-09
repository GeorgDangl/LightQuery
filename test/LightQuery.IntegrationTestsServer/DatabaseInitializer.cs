using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightQuery.IntegrationTestsServer
{
    public class DatabaseInitializer
    {
        public DatabaseInitializer(LightQueryContext context)
        {
            _context = context;
        }

        private readonly LightQueryContext _context;

        public async Task InitializeDatabase()
        {
            var users = GetInitialUsers();
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }

        public static List<User> GetInitialUsers()
        {
            return new List<User>
            {
                new User {Id = 1, UserName = "Alice", Email = "alice@example.com"},
                new User {Id = 2, UserName = "Bob", Email = "bob@example.com"},
                new User {Id = 3, UserName = "Caroline", Email = "caroline@example.com"},
                new User {Id = 4, UserName = "Dave", Email = "dave@example.com"},
                new User {Id = 5, UserName = "Emilia", Email = "emilia@example.com"},
                new User {Id = 6, UserName = "Fred", Email = "fred@example.com"},
                new User {Id = 7, UserName = "Georgia", Email = "georgia@example.com"},
                new User {Id = 8, UserName = "Hank", Email = "hank@example.com"},
                new User {Id = 9, UserName = "Iris", Email = "iris@example.com"},
                new User {Id = 10, UserName = "Joe", Email = "joe@example.com"}
            };
        }
    }
}
