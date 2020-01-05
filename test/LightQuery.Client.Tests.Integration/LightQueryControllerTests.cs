using System.Net.Http;
using System.Threading.Tasks;
using LightQuery.IntegrationTestsServer;
using Newtonsoft.Json;
using Xunit;

namespace LightQuery.Client.Tests.Integration
{
    public class LightQueryControllerTests
    {
        private readonly HttpClient _client = IntegrationTestServer.GetTestServer().CreateClient();

        private async Task<PaginationResult<T>> GetResponse<T>(string query)
        {
            var response = await _client.GetAsync($"LightQuery{query}");
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
        public async Task SortByEmailWithPascalCase()
        {
            var query = QueryBuilder.Build(sortParam: "Email");
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

        [Fact]
        public async Task SortByEmailDescendingWithPascalCase()
        {
            var query = QueryBuilder.Build(sortParam: "Email", sortDescending: true);
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].Email.CompareTo(actualResponse.Data[i - 1].Email) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByGenderThenByUserName()
        {
            var query = QueryBuilder.Build(sortParam: "gender", thenSortParam: "userName");
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);

            Assert.Equal(10, actualResponse.Data.Count);
            Assert.Equal("F", actualResponse.Data[0].Gender);
            Assert.Equal("F", actualResponse.Data[1].Gender);
            Assert.Equal("F", actualResponse.Data[2].Gender);
            Assert.Equal("F", actualResponse.Data[3].Gender);
            Assert.Equal("F", actualResponse.Data[4].Gender);
            Assert.Equal("M", actualResponse.Data[5].Gender);
            Assert.Equal("M", actualResponse.Data[6].Gender);
            Assert.Equal("M", actualResponse.Data[7].Gender);
            Assert.Equal("M", actualResponse.Data[8].Gender);
            Assert.Equal("M", actualResponse.Data[9].Gender);

            Assert.Equal("Alice", actualResponse.Data[0].UserName);
            Assert.Equal("Caroline", actualResponse.Data[1].UserName);
            Assert.Equal("Emilia", actualResponse.Data[2].UserName);
            Assert.Equal("Georgia", actualResponse.Data[3].UserName);
            Assert.Equal("Iris", actualResponse.Data[4].UserName);
            Assert.Equal("Bob", actualResponse.Data[5].UserName);
            Assert.Equal("Dave", actualResponse.Data[6].UserName);
            Assert.Equal("Fred", actualResponse.Data[7].UserName);
            Assert.Equal("Hank", actualResponse.Data[8].UserName);
            Assert.Equal("Joe", actualResponse.Data[9].UserName);
        }

        [Fact]
        public async Task SortByGenderThenByUserNameDescending()
        {
            var query = QueryBuilder.Build(sortParam: "gender", thenSortParam: "userName", thenSortDescending: true);
            var actualResponse = await GetResponse<User>(query);
            Assert.NotNull(actualResponse);

            Assert.Equal(10, actualResponse.Data.Count);
            Assert.Equal("F", actualResponse.Data[0].Gender);
            Assert.Equal("F", actualResponse.Data[1].Gender);
            Assert.Equal("F", actualResponse.Data[2].Gender);
            Assert.Equal("F", actualResponse.Data[3].Gender);
            Assert.Equal("F", actualResponse.Data[4].Gender);
            Assert.Equal("M", actualResponse.Data[5].Gender);
            Assert.Equal("M", actualResponse.Data[6].Gender);
            Assert.Equal("M", actualResponse.Data[7].Gender);
            Assert.Equal("M", actualResponse.Data[8].Gender);
            Assert.Equal("M", actualResponse.Data[9].Gender);

            Assert.Equal("Iris", actualResponse.Data[0].UserName);
            Assert.Equal("Georgia", actualResponse.Data[1].UserName);
            Assert.Equal("Emilia", actualResponse.Data[2].UserName);
            Assert.Equal("Caroline", actualResponse.Data[3].UserName);
            Assert.Equal("Alice", actualResponse.Data[4].UserName);
            Assert.Equal("Joe", actualResponse.Data[5].UserName);
            Assert.Equal("Hank", actualResponse.Data[6].UserName);
            Assert.Equal("Fred", actualResponse.Data[7].UserName);
            Assert.Equal("Dave", actualResponse.Data[8].UserName);
            Assert.Equal("Bob", actualResponse.Data[9].UserName);
        }
    }
}
