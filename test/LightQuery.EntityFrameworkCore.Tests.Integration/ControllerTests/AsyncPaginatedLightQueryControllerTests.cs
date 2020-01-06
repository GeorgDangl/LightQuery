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
            // There is no fifth page for the given page size,
            // so it should return the last (fourth) page
            var url = "AsyncPaginatedLightQuery?sort=userName&page=5";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(4, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            // There's only a single record left on the fourth page
            Assert.Single(pagedResult.Data);
            Assert.Contains(pagedResult.Data, u => u.UserName == "Joe");
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
        public async Task IfPageAndPageSizeExceedTotalCountReturnsLastValidPageAndRecord()
        {
            var url = "AsyncPaginatedLightQuery?sort=userName&page=4&pageSize=5";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);

            Assert.Equal(2, pagedResult.Page);
            Assert.Equal(5, pagedResult.PageSize);
            Assert.Equal(5, pagedResult.Data.Count);
        }

        [Fact]
        public async Task ReturnsEmptyResultWithPageSetToOneIfNoRecordsPresent()
        {
            var url = "AsyncPaginatedLightQuery?returnEmptyList=true&pageSize=5";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(0, pagedResult.TotalCount);

            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(5, pagedResult.PageSize);
            Assert.Empty(pagedResult.Data);
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

        [Fact]
        public async Task CanSortByNestedProperty()
        {
            var url = "AsyncPaginatedLightQueryWithDefaultSort?sort=favoriteAnimal.name&page=1";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Equal("Joe", pagedResult.Data[0].UserName);
            Assert.Equal("Iris", pagedResult.Data[1].UserName);
            Assert.Equal("Hank", pagedResult.Data[2].UserName);
        }

        [Fact]
        public async Task CanSortByNestedPropertyDescending()
        {
            var url = "AsyncPaginatedLightQueryWithDefaultSort?sort=favoriteAnimal.name desc&page=1";
            var pagedResult = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(10, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(3, pagedResult.PageSize);
            Assert.Equal(3, pagedResult.Data.Count);
            Assert.Equal("Alice", pagedResult.Data[0].UserName);
            Assert.Equal("Bob", pagedResult.Data[1].UserName);
            Assert.Equal("Caroline", pagedResult.Data[2].UserName);
        }

        [Fact]
        public async Task PreservesBadRequestResponse()
        {
            var url = "AsyncPaginatedLightQueryWithBadRequestResponse";
            var response = await GetResponseMessage(url);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SortByGenderThenByUserName()
        {
            var url = "AsyncPaginatedLightQuery?sort=gender&thenSort=UserName&pageSize=10";
            var actualResponse = await GetResponse<PaginationResult<User>>(url);

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
            var url = "AsyncPaginatedLightQuery?sort=gender&thenSort=UserName desc&pageSize=10";
            var actualResponse = await GetResponse<PaginationResult<User>>(url);

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
