using System.Linq;
using System.Threading.Tasks;
using LightQuery.Client;
using LightQuery.IntegrationTestsServer;
using Xunit;

namespace LightQuery.EntityFrameworkCore.Tests.Integration.ControllerTests
{
    public class AsyncPaginatedLightQueryControllerTests : ControllerTestBase
    {
        [Fact]
        public async Task PaginatesWithoutQuery()
        {
            var url = "AsyncPaginatedLightQuery";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
        }

        [Fact]
        public async Task SortByUserNameAndFirstPage()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=1";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
        }

        [Fact]
        public async Task SortByUserNameAndFirstPageWithCamelCase()
        {
            var url = "AsyncPaginatedLightQuery?sort=UserName&page=1";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
        }

        [Fact]
        public async Task SortByUserNameAndSecondPage()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=2";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(2, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Dave");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Emilia");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Fred");
        }

        [Fact]
        public async Task SortByUserNameAndFourthPage()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=4";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(4, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Single(pagedResult.Data);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Joe");
        }

        [Fact]
        public async Task SortByUserNameAndFifthPage()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=5";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(5, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Empty(pagedResult.Data);
        }

        [Fact]
        public async Task SortByUsernameAndReturnAllWhenPageSizeExceedsItemCount()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&pageSize=15";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(15, pagedResult.PageSize);
            Assert.Equal(10, pagedResult.Data.Count);
        }

        [Fact]
        public async Task DefaultPaginationWithoutPaginationParameter()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
        }

        [Fact]
        public async Task TakesUserDefinedSmallerPageSize()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=1&pageSize=2";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(2, pagedResult.PageSize);
            Assert.Equal(2, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
        }

        [Fact]
        public async Task TakesUserDefinedBiggerPageSize()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=1&pageSize=4";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(4, pagedResult.PageSize);
            Assert.Equal(4, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Dave");
        }

        [Fact]
        public async Task DefaultResponseWithNegativePage()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=-2";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
        }

        [Fact]
        public async Task DefaultResponseWithNegativePageSize()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=1&pageSize=-4";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
        }

        [Fact]
        public async Task DefaultResponseWithNaNPage()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=giveMeTheFirstPagePlease";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
        }

        [Fact]
        public async Task DefaultResponseWithNaNPageSize()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=1&pageSize=biggerThanTheLastOne";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
        }

        [Fact]
        public async Task AppliesDefaultSortWithoutClientSortParameter()
        {
            var url = "AsyncPaginatedLightQueryWithDefaultSort";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.Email == "alice@example.com");
            Assert.Contains(pagedResult.Data, u => u.Email == "bob@example.com");
            Assert.Contains(pagedResult.Data, u => u.Email == "caroline@example.com");
        }

        [Fact]
        public async Task CanOverrideDefaultSort()
        {
            var url = "AsyncPaginatedLightQueryWithDefaultSort?sort=userName&page=1";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Alice");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Bob");
            Assert.Contains(pagedResult.Data, u => u.UserName == "Caroline");
        }
    }
}
