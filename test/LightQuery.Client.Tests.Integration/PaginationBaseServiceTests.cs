using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
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
                : this((url, _) => getHttpAsync(url), options)
            { }

            public UsersService(Func<string, CancellationToken, Task<HttpResponseMessage>> getHttpAsync,
                DefaultPaginationOptions options = null)
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
            var requestsSent = 0;
            var service = new UsersService((url, cancellationToken) =>
            {
                requestsSent++;
                return _client.GetAsync(url, cancellationToken);
            });

            service.PaginationResult.Subscribe(_ => results++);

            await Task.WhenAny(service.PaginationResult.FirstAsync().ToTask(),
                 Task.Delay(10_000)); // Timeout

            Assert.Equal(1, results);
            Assert.Equal(1, requestsSent);

            service.SetSortProperty("Email");
            service.SetSortProperty("UserName");

            await Task.WhenAny(service.PaginationResult.Skip(1).FirstAsync().ToTask(),
                Task.Delay(10_000)); // Timeout

            Assert.Equal(2, results);
            Assert.Equal(3, requestsSent);
        }

        [Fact]
        public async Task CancelsFirstRequestWhenSecondOneIsSent_PageChanged()
        {
            var results = 0;
            var requestsSent = 0;
            var service = new UsersService((url, cancellationToken) =>
            {
                requestsSent++;
                return _client.GetAsync(url, cancellationToken);
            });

            service.PaginationResult.Subscribe(_ => results++);

            await Task.WhenAny(service.PaginationResult.FirstAsync().ToTask(),
                 Task.Delay(10_000)); // Timeout

            Assert.Equal(1, results);
            Assert.Equal(1, requestsSent);

            service.Page++;
            service.Page++;

            await Task.WhenAny(service.PaginationResult.Skip(1).FirstAsync().ToTask(),
                Task.Delay(10_000)); // Timeout

            Assert.Equal(2, results);
            Assert.Equal(3, requestsSent);
        }

        [Fact]
        public async Task CancelsFirstRequestWhenSecondOneIsSent_UsesCancellationToken()
        {
            var results = 0;
            var requestsSent = 0;
            var requestsCancelled = new bool[3];
            var responseTasks = new List<Task>();
            var currentRequests = 0;


            var service = new UsersService(async (url, cancellationToken) =>
            {
                requestsSent++;
                var currentRequest = currentRequests++;
                try
                {
                    var resultTask = _client.GetAsync(url, cancellationToken);
                    await Task.Delay(100); // Need a small delay in case the server responds too quickly,
                    // thus the request not being cancelled while it's en-route and therefore not throwing
                    // an exception
                    var result = await resultTask;
                    requestsCancelled[currentRequest] = false;
                    return result;
                }
                catch
                {
                    requestsCancelled[currentRequest] = true;
                    return null;
                }
            });

            service.PaginationResult.Subscribe(_ => results++);

            await Task.WhenAny(service.PaginationResult.FirstAsync().ToTask(),
                 Task.Delay(10_000)); // Timeout

            Assert.Equal(1, results);
            Assert.Equal(1, requestsSent);

            service.Page++;
            service.Page++;

            await Task.WhenAny(service.PaginationResult.Skip(1).FirstAsync().ToTask(),
                Task.Delay(10_000)); // Timeout

            Assert.Equal(2, results);
            Assert.Equal(3, requestsSent);

            Assert.False(requestsCancelled[0]);
            Assert.True(requestsCancelled[1]);
            Assert.False(requestsCancelled[2]);
        }
    }
}
