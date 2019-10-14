using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightQuery.Client;
using LightQuery.IntegrationTestsServer;
using Xunit;

namespace LightQuery.Tests.Integration.ControllerTests
{
    public class LightQueryControllerTests : ControllerTestBase
    {
        [Fact]
        public async Task DoesNotPaginateWithoutQuery()
        {
            var url = "LightQuery";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.NotNull(actualResponse);
            Assert.IsType<List<User>>(actualResponse);
        }

        [Fact]
        public async Task SortById()
        {
            var url = "LightQuery?sort=id";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].Id > actualResponse[i - 1].Id;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByIdWithCamelCase()
        {
            var url = "LightQuery?sort=Id";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].Id > actualResponse[i - 1].Id;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByIdDescending()
        {
            var url = "LightQuery?sort=id desc";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].Id > actualResponse[i-1].Id;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByUserName()
        {
            var url = "LightQuery?sort=userName";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].UserName.CompareTo(actualResponse[i - 1].UserName) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByUserNameWithCamelCase()
        {
            var url = "LightQuery?sort=UserName";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].UserName.CompareTo(actualResponse[i - 1].UserName) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByUserNameDescending()
        {
            var url = "LightQuery?sort=userName desc";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].UserName.CompareTo(actualResponse[i - 1].UserName) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByUserNameDescendingWithCamelCase()
        {
            var url = "LightQuery?sort=UserName desc";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].UserName.CompareTo(actualResponse[i - 1].UserName) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByEmail()
        {
            var url = "LightQuery?sort=email";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].Email.CompareTo(actualResponse[i - 1].Email) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByEmailDescending()
        {
            var url = "LightQuery?sort=email desc";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].Email.CompareTo(actualResponse[i - 1].Email) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByRegistrationDate()
        {
            var url = "LightQuery?sort=registrationDate";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].RegistrationDate.CompareTo(actualResponse[i - 1].RegistrationDate) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByRegistrationDateDescending()
        {
            var url = "LightQuery?sort=registrationDate desc";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].RegistrationDate.CompareTo(actualResponse[i - 1].RegistrationDate) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByRegistrationDateWithPagination()
        {
            var url = "LightQuery?sort=registrationDate&page=2&pageSize=3";
            var actualResponse = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(2, actualResponse.Page);
            Assert.Equal(3, actualResponse.PageSize);
            Assert.Equal(3, actualResponse.Data.Count);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].RegistrationDate.CompareTo(actualResponse.Data[i - 1].RegistrationDate) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByRegistrationDateDescendingWithPagination()
        {
            var url = "LightQuery?sort=registrationDate desc&page=2&pageSize=3";
            var actualResponse = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(2, actualResponse.Page);
            Assert.Equal(3, actualResponse.PageSize);
            Assert.Equal(3, actualResponse.Data.Count);
            for (var i = 1; i < actualResponse.Data.Count; i++)
            {
                var previousValueIsSmaller = actualResponse.Data[i].RegistrationDate.CompareTo(actualResponse.Data[i - 1].RegistrationDate) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task SortByLastLoginDate()
        {
            var url = "LightQuery?sort=registrationDate";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.Single(actualResponse.Where(u => u.LastLoginDate != null));
        }

        [Fact]
        public async Task SortByLastLoginDateDescending()
        {
            var url = "LightQuery?sort=registrationDate desc";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.Single(actualResponse.Where(u => u.LastLoginDate != null));
        }

        [Fact]
        public async Task SortByLastLoginDateWithPagination()
        {
            var url = "LightQuery?sort=registrationDate&page=2&pageSize=3";
            var actualResponse = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(2, actualResponse.Page);
            Assert.Equal(3, actualResponse.PageSize);
            Assert.Single(actualResponse.Data.Where(u => u.LastLoginDate != null));
        }

        [Fact]
        public async Task SortByLastLoginDateDescendingWithPagination()
        {
            var url = "LightQuery?sort=registrationDate desc&page=2&pageSize=3";
            var actualResponse = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(2, actualResponse.Page);
            Assert.Equal(3, actualResponse.PageSize);
            Assert.Empty(actualResponse.Data.Where(u => u.LastLoginDate != null));
        }

        [Fact]
        public async Task DontSortWithoutSortParameter()
        {
            var url = "LightQuery";
            var response1 = await GetResponse<List<User>>(url);
            var response2 = await GetResponse<List<User>>(url);
            var response3 = await GetResponse<List<User>>(url);
            var response4 = await GetResponse<List<User>>(url);
            Func<IEnumerable<User>, string> aggregateEmails = users =>
                users
                    .Select(u => u.Email)
                    .Aggregate((current, next) => current + next);
            var firstAggregate = aggregateEmails(response1) + aggregateEmails(response2);
            var secondAggregate = aggregateEmails(response3) + aggregateEmails(response4);
            Assert.NotEqual(firstAggregate, secondAggregate);
        }

        [Fact]
        public async Task DontSortWithInvalidSortParameter()
        {
            var url = "LightQuery?sort=firstName";
            var response1 = await GetResponse<List<User>>(url);
            var response2 = await GetResponse<List<User>>(url);
            var response3 = await GetResponse<List<User>>(url);
            var response4 = await GetResponse<List<User>>(url);
            Func<IEnumerable<User>, string> aggregateEmails = users =>
                users
                    .Select(u => u.Email)
                    .Aggregate((current, next) => current + next);
            var firstAggregate = aggregateEmails(response1) + aggregateEmails(response2);
            var secondAggregate = aggregateEmails(response3) + aggregateEmails(response4);
            Assert.NotEqual(firstAggregate, secondAggregate);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageWithSortParam()
        {
            var url = "LightQuery?sort=userName&page=1";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageSizeWithSortParam()
        {
            var url = "LightQuery?sort=userName&pageSize=2";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageAndPageSizeWithSortParam()
        {
            var url = "LightQuery?sort=userName&page=2&pageSize=3";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageWithoutSortParam()
        {
            var url = "LightQuery?page=1";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageSizeWithoutSortParam()
        {
            var url = "LightQuery?pageSize=2";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageAndPageSizeWithoutSortParam()
        {
            var url = "LightQuery?page=2&pageSize=3";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task AppliesDefaultSortWithoutClientSortParameter()
        {
            var url = "LightQueryWithDefaultSort";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].Email.CompareTo(actualResponse[i - 1].Email) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task CanOverrideDefaultSort()
        {
            var url = "LightQueryWithDefaultSort?sort=userName";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].UserName.CompareTo(actualResponse[i - 1].UserName) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task CanFilterByNestedProperty()
        {
            var url = "LightQueryWithDefaultSort?sort=favoriteAnimal.name";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].UserName.CompareTo(actualResponse[i - 1].UserName) > 0;
                Assert.False(previousValueIsSmaller);
            }
        }
    }
}
