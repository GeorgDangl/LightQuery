using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using LightQuery.IntegrationTestsServer;
using Newtonsoft.Json;
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

        [Fact]
        public async Task GetsUsersOnInitializationSortedByGenderThenByUsername()
        {
            var service = GetService(new DefaultPaginationOptions
            {
                SortProperty = "Gender",
                ThenSortProperty = "UserName"
            });
            var serviceResult = await service.PaginationResult
                .Timeout(_testResultTimeout)
                .FirstAsync();
            Assert.NotNull(serviceResult);
            Assert.Equal(1, serviceResult.Page);
            Assert.Equal(20, serviceResult.PageSize);
            Assert.NotNull(serviceResult.Data);

            Assert.Equal(10, serviceResult.Data.Count);
            Assert.Equal("F", serviceResult.Data[0].Gender);
            Assert.Equal("F", serviceResult.Data[1].Gender);
            Assert.Equal("F", serviceResult.Data[2].Gender);
            Assert.Equal("F", serviceResult.Data[3].Gender);
            Assert.Equal("F", serviceResult.Data[4].Gender);
            Assert.Equal("M", serviceResult.Data[5].Gender);
            Assert.Equal("M", serviceResult.Data[6].Gender);
            Assert.Equal("M", serviceResult.Data[7].Gender);
            Assert.Equal("M", serviceResult.Data[8].Gender);
            Assert.Equal("M", serviceResult.Data[9].Gender);

            Assert.Equal("Alice", serviceResult.Data[0].UserName);
            Assert.Equal("Caroline", serviceResult.Data[1].UserName);
            Assert.Equal("Emilia", serviceResult.Data[2].UserName);
            Assert.Equal("Georgia", serviceResult.Data[3].UserName);
            Assert.Equal("Iris", serviceResult.Data[4].UserName);
            Assert.Equal("Bob", serviceResult.Data[5].UserName);
            Assert.Equal("Dave", serviceResult.Data[6].UserName);
            Assert.Equal("Fred", serviceResult.Data[7].UserName);
            Assert.Equal("Hank", serviceResult.Data[8].UserName);
            Assert.Equal("Joe", serviceResult.Data[9].UserName);
        }

        [Fact]
        public async Task CancelsFirstRequestWhenSecondOneIsSent_SortPropertyChanged()
        {
            var results = 0;
            Func<string, Task<HttpResponseMessage>> getHttpAsync = _ => GetResponseWithDelay();
            var paginationService = new PaginationBaseService<User>("https://example.com", getHttpAsync);
            paginationService.PaginationResult.Subscribe(_ => results++);
            await Task.Delay(100);

            Assert.Equal(1, results);

            paginationService.SetSortProperty("Email");
            paginationService.SetSortProperty("UserName");

            await Task.Delay(100);
            Assert.Equal(2, results);
        }

        [Fact]
        public async Task CancelsFirstRequestWhenSecondOneIsSent_PageChanged()
        {
            var results = 0;
            Func<string, Task<HttpResponseMessage>> getHttpAsync = _ => GetResponseWithDelay();
            var paginationService = new PaginationBaseService<User>("https://example.com", getHttpAsync);
            paginationService.PaginationResult.Subscribe(_ => results++);
            await Task.Delay(100);

            Assert.Equal(1, results);

            paginationService.Page++;
            paginationService.Page++;

            await Task.Delay(100);
            Assert.Equal(2, results);
        }

        private async Task<HttpResponseMessage> GetResponseWithDelay()
        {
            await Task.Delay(10);

            var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var memStream = new MemoryStream();
            var usersPaginated = new PaginationResult<User>
            {
                Page = 1,
                PageSize = 10,
                TotalCount = 1,
                Data = new System.Collections.Generic.List<User>
                {
                    new User
                    {
                        Email = "george@example.com"
                    }
                }
            };
            var usersJson = JsonConvert.SerializeObject(usersPaginated);
            using (var sw = new StreamWriter(memStream, Encoding.UTF8, 2048, true))
            {
                await sw.WriteAsync(usersJson);
            }

            memStream.Position = 0;
            httpResponse.Content = new StreamContent(memStream);
            return httpResponse;
        }
    }
}
