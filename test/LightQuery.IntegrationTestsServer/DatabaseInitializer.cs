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
                new User {Id = 1, UserName = "Alice", Email = "alice@example.com", FavoriteAnimal = new Animal{ Id = 1, Name = "Jaguar" } },
                new User {Id = 2, UserName = "Bob", Email = "bob@example.com", FavoriteAnimal = new Animal{ Id = 2, Name = "Impala" }},
                new User {Id = 3, UserName = "Caroline", Email = "caroline@example.com", FavoriteAnimal = new Animal{ Id = 3, Name = "Hornet" }},
                new User {Id = 4, UserName = "Dave", Email = "dave@example.com", FavoriteAnimal = new Animal{ Id = 4, Name = "Giraffe" }},
                new User {Id = 5, UserName = "Emilia", Email = "emilia@example.com", FavoriteAnimal = new Animal{ Id = 5, Name = "Ferret" }},
                new User {Id = 6, UserName = "Fred", Email = "fred@example.com", FavoriteAnimal = new Animal{ Id = 6, Name = "Eagle" }},
                new User {Id = 7, UserName = "Georgia", Email = "georgia@example.com", FavoriteAnimal = new Animal{ Id = 7, Name = "Donkey" }},
                new User {Id = 8, UserName = "Hank", Email = "hank@example.com", FavoriteAnimal = new Animal{ Id = 8, Name = "Cow" }},
                new User {Id = 9, UserName = "Iris", Email = "iris@example.com", FavoriteAnimal = new Animal{ Id = 9, Name = "Beaver" }},
                new User {Id = 10, UserName = "Joe", Email = "joe@example.com", FavoriteAnimal = new Animal{ Id = 10, Name = "Alpaca" }}
            };
        }
    }
}
