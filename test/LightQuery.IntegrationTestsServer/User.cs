using System;

namespace LightQuery.IntegrationTestsServer
{
    public class User
    {
        public Guid RandomSortParamter { get; set; } = Guid.NewGuid();
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public Animal FavoriteAnimal { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
