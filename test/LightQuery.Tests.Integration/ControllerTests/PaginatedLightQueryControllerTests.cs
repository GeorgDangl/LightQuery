using System.Linq;
using System.Threading.Tasks;
using LightQuery.Client;
using LightQuery.IntegrationTestsServer;
using Xunit;

namespace LightQuery.Tests.Integration.ControllerTests
{
    public class PaginatedLightQueryControllerTests : ControllerTestBase
    {
        [Fact]
        public async Task PaginatesWithoutQuery()
        {
            var url = "PaginatedLightQuery";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
        }

        [Fact]
        public async Task SortByUserNameAndFirstPage()
        {
            var url = "PaginatedLightQuery?sort=userName&page=1";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Caroline"));
        }

        [Fact]
        public async Task SortByUserNameAndFirstPageWithCamelCase()
        {
            var url = "PaginatedLightQuery?sort=UserName&page=1";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Caroline"));
        }

        [Fact]
        public async Task SortByUserNameAndSecondPage()
        {
            var url = "PaginatedLightQuery?sort=userName&page=2";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(2, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Dave"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Emilia"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Fred"));
        }

        [Fact]
        public async Task SortByUserNameAndFourthPage()
        {
            var url = "PaginatedLightQuery?sort=userName&page=4";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(4, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(1, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Joe"));
        }

        [Fact]
        public async Task SortByUserNameAndFifthPage()
        {
            var url = "PaginatedLightQuery?sort=userName&page=5";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(5, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(0, pagedResult.Data.Count);
        }

        [Fact]
        public async Task SortByUsernameAndReturnAllWhenPageSizeExceedsItemCount()
        {
            var url = "PaginatedLightQuery?sort=userName&pageSize=15";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(15, pagedResult.PageSize);
            Assert.Equal(10, pagedResult.Data.Count);
        }

        [Fact]
        public async Task DefaultPaginationWithoutPaginationParameter()
        {
            var url = "PaginatedLightQuery?sort=userName";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Caroline"));
        }

        [Fact]
        public async Task TakesUserDefinedSmallerPageSize()
        {
            var url = "PaginatedLightQuery?sort=userName&page=1&pageSize=2";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(2, pagedResult.PageSize);
            Assert.Equal(2, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
        }

        [Fact]
        public async Task TakesUserDefinedBiggerPageSize()
        {
            var url = "PaginatedLightQuery?sort=userName&page=1&pageSize=4";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(4, pagedResult.PageSize);
            Assert.Equal(4, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Caroline"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Dave"));
        }

        [Fact]
        public async Task DefaultResponseWithNegativePage()
        {
            var url = "PaginatedLightQuery?sort=userName&page=-2";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Caroline"));
        }

        [Fact]
        public async Task DefaultResponseWithNegativePageSize()
        {
            var url = "PaginatedLightQuery?sort=userName&page=1&pageSize=-4";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Caroline"));
        }

        [Fact]
        public async Task DefaultResponseWithNaNPage()
        {
            var url = "PaginatedLightQuery?sort=userName&page=giveMeTheFirstPagePlease";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Caroline"));
        }

        [Fact]
        public async Task DefaultResponseWithNaNPageSize()
        {
            var url = "PaginatedLightQuery?sort=userName&page=1&pageSize=biggerThanTheLastOne";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Alice"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Bob"));
            Assert.True(pagedResult.Data.Any(u => u.UserName == "Caroline"));
        }
    }
}
