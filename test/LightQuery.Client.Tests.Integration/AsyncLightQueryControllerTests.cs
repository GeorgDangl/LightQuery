using System.Net.Http;
using System.Threading.Tasks;
using LightQuery.IntegrationTestsServer;
using Newtonsoft.Json;
using Xunit;

namespace LightQuery.Client.Tests.Integration
{
    public class AsyncLightQueryControllerTests
    {
        private readonly HttpClient _client = IntegrationTestServer.GetTestServer().CreateClient();

        private async Task<PaginationResult<T>> GetResponse<T>(string query)
        {
            var response = await _client.GetAsync($"AsyncLightQuery{query}");
            Assert.True(response.IsSuccessStatusCode, "The HttpResponse indicates a non-success status code");
            var responseContent = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<PaginationResult<T>>(responseContent);
            return deserializedResponse;
        }

        [Fact]
        public async Task SortById()
        {
            var query = QueryBuilder.Build(sortParam: "id");
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].Id > actualResponse.Data[i - 1].Id;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByIdDescending()
        {
            var query = QueryBuilder.Build(sortParam: "id", sortDescending: true);
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].Id > actualResponse.Data[i - 1].Id;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByUserName()
        {
            var query = QueryBuilder.Build(sortParam: "userName");
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].UserName.CompareTo(actualResponse.Data[i - 1].UserName) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByUserNameWithPascalCase()
        {
            var query = QueryBuilder.Build(sortParam: "UserName");
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].UserName.CompareTo(actualResponse.Data[i - 1].UserName) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByUserNameDescending()
        {
            var query = QueryBuilder.Build(sortParam: "userName", sortDescending: true);
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].UserName.CompareTo(actualResponse.Data[i - 1].UserName) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByEmail()
        {
            var query = QueryBuilder.Build(sortParam: "email");
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].Email.CompareTo(actualResponse.Data[i - 1].Email) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByEmailDescending()
        {
            var query = QueryBuilder.Build(sortParam: "email", sortDescending: true);
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].Email.CompareTo(actualResponse.Data[i - 1].Email) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }
    }
}
