using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using LightQuery.IntegrationTestsServer;
using Xunit;

namespace LightQuery.Client.Tests.Integration
{
    public class PaginationBaseServiceTests
    {
        private readonly HttpClient _client = IntegrationTestServer.GetTestServer().CreateClient();
        private readonly TimeSpan _testResultTimeout = TimeSpan.FromSeconds(60);

        private class UsersService : PaginationBaseService<User>
        {
            public UsersService(Func<string, Task<HttpResponseMessage>> getHttpAsync, DefaultPaginationOptions options = null)
                : base("AsyncLightQuery", getHttpAsync, options)
            { }
        }

        private UsersService GetService(DefaultPaginationOptions options = null)
        {
            var service = new UsersService(url => _client.GetAsync(url), options);
            return service;
        }

        [Fact]
        public async Task GetsUsersOnInitializationSortedByUsername()
        {
            var service = GetService(new DefaultPaginationOptions {SortProperty = "UserName" });
            var serviceResult = await service.PaginationResult
                .Timeout(_testResultTimeout)
                .FirstAsync();
            Assert.NotNull(serviceResult);
            Assert.Equal(1, serviceResult.Page);
            Assert.Equal(20, serviceResult.PageSize);
            Assert.NotNull(serviceResult.Data);
            Assert.Equal(10, serviceResult.Data.Count);
            Assert.Contains(serviceResult.Data, u => u.UserName == "Alice");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Bob");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Caroline");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Dave");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Emilia");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Fred");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Georgia");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Hank");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Iris");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Joe");
        }

        [Fact]
        public async Task GetsUsersOnPageTwoWithPageSizeThreeSortedByUsername()
        {
            var service = GetService(new DefaultPaginationOptions {Page = 2, PageSize = 3, SortProperty = "UserName" });
            var serviceResult = await service.PaginationResult
                .Timeout(_testResultTimeout)
                .FirstAsync();
            Assert.NotNull(serviceResult);
            Assert.Equal(2, serviceResult.Page);
            Assert.Equal(3, serviceResult.PageSize);
            Assert.NotNull(serviceResult.Data);
            Assert.Equal(3, serviceResult.Data.Count);
            Assert.Contains(serviceResult.Data, u => u.UserName == "Dave");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Emilia");
            Assert.Contains(serviceResult.Data, u => u.UserName == "Fred");
        }
    }
}
