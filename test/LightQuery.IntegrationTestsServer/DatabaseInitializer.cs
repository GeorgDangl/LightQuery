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
                new User {Id = 1, UserName = "Alice", Email = "alice@example.com", FavoriteAnimal = new Animal{ Id = 1, Name = "Jaguar" }, RegistrationDate = new System.DateTime(2019,10,14), LastLoginDate = new System.DateTime(2019,10,15), Gender = "F" },
                new User {Id = 2, UserName = "Bob", Email = "bob@example.com", FavoriteAnimal = new Animal{ Id = 2, Name = "Impala" }, RegistrationDate = new System.DateTime(2019,10,15), Gender = "M"},
                new User {Id = 3, UserName = "Caroline", Email = "caroline@example.com", FavoriteAnimal = new Animal{ Id = 3, Name = "Hornet" }, RegistrationDate = new System.DateTime(2019,10,16), Gender = "F"},
                new User {Id = 4, UserName = "Dave", Email = "dave@example.com", FavoriteAnimal = new Animal{ Id = 4, Name = "Giraffe" }, RegistrationDate = new System.DateTime(2019,9,17), Gender = "M"},
                new User {Id = 5, UserName = "Emilia", Email = "emilia@example.com", FavoriteAnimal = new Animal{ Id = 5, Name = "Ferret" }, RegistrationDate = new System.DateTime(2019,9,18), Gender = "F"},
                new User {Id = 6, UserName = "Fred", Email = "fred@example.com", FavoriteAnimal = new Animal{ Id = 6, Name = "Eagle" }, RegistrationDate = new System.DateTime(2019,9,19), Gender = "M"},
                new User {Id = 7, UserName = "Georgia", Email = "georgia@example.com", FavoriteAnimal = new Animal{ Id = 7, Name = "Donkey" }, RegistrationDate = new System.DateTime(2019,11,20), Gender = "F"},
                new User {Id = 8, UserName = "Hank", Email = "hank@example.com", FavoriteAnimal = new Animal{ Id = 8, Name = "Cow" }, RegistrationDate = new System.DateTime(2019,11,21), Gender = "M"},
                new User {Id = 9, UserName = "Iris", Email = "iris@example.com", FavoriteAnimal = new Animal{ Id = 9, Name = "Beaver" }, RegistrationDate = new System.DateTime(2019,11,22), Gender = "F"},
                new User {Id = 10, UserName = "Joe", Email = "joe@example.com", FavoriteAnimal = new Animal{ Id = 10, Name = "Alpaca" }, RegistrationDate = new System.DateTime(2019,10,23), Gender = "M"}
            };
        }
    }
}
