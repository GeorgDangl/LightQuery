using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LightQuery.Client.Tests
{
    public class MockPaginationService : PaginationBaseService<MockPayload>
    {
        public MockPaginationService(string baseUrl,
            Func<string, Task<HttpResponseMessage>> getHttpAsync,
            DefaultPaginationOptions options = null)
                : base(baseUrl, getHttpAsync, options)
        { }
    }

    public class MockPayload
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }
    
    public class PaginationBaseServiceTests
    {
        [Fact]
        public void AcceptsNullBaseUrl()
        {
            var service = new MockPaginationService(null, x => Task.FromResult(new HttpResponseMessage()));
            Assert.NotNull(service);
        }
        
        [Fact]
        public void SetsBaseUrlToStringEmptyForEmptyBaseUrl()
        {
            var service = new MockPaginationService(null, x => Task.FromResult(new HttpResponseMessage()));
            Assert.Equal(string.Empty, service.BaseUrl);
        }

        [Fact]
        public void AcceptsEmptyBaseUrl()
        {
            var service = new MockPaginationService(string.Empty, x => Task.FromResult(new HttpResponseMessage()));
            Assert.NotNull(service);
        }

        [Fact]
        public void AcceptsAbsoluteBaseUrl()
        {
            var baseUrl = "https://example.com";
            var service = new MockPaginationService(baseUrl, x => Task.FromResult(new HttpResponseMessage()));
            Assert.Equal(baseUrl, service.BaseUrl);
        }

        [Fact]
        public void AcceptsRelativeBaseUrl()
        {
            var baseUrl = "/api/values";
            var service = new MockPaginationService(baseUrl, x => Task.FromResult(new HttpResponseMessage()));
            Assert.Equal(baseUrl, service.BaseUrl);
        }

        [Fact]
        public void ArgumentNullExceptionForNullGetHttpAsyncAction()
        {
            Assert.Throws<ArgumentNullException>("getHttpAsync", () => new MockPaginationService(string.Empty, null));
        }

    }
}
