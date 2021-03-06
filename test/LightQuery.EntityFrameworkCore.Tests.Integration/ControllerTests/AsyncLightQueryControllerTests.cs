using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightQuery.Client;
using LightQuery.IntegrationTestsServer;
using Xunit;

namespace LightQuery.EntityFrameworkCore.Tests.Integration.ControllerTests
{
    public class AsyncLightQueryControllerTests : ControllerTestBase
    {
        [Fact]
        public async Task DoesNotPaginateWithoutQuery()
        {
            var url = "AsyncLightQuery";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.NotNull(actualResponse);
            Assert.IsType<List<User>>(actualResponse);
        }

        [Fact]
        public async Task SortById()
        {
            var url = "AsyncLightQuery?sort=id";
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
            var url = "AsyncLightQuery?sort=Id";
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
            var url = "AsyncLightQuery?sort=id desc";
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
            var url = "AsyncLightQuery?sort=userName";
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
            var url = "AsyncLightQuery?sort=UserName";
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
            var url = "AsyncLightQuery?sort=userName desc";
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
            var url = "AsyncLightQuery?sort=UserName desc";
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
            var url = "AsyncLightQuery?sort=email";
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
            var url = "AsyncLightQuery?sort=email desc";
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
            var url = "AsyncLightQuery?sort=registrationDate";
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
            var url = "AsyncLightQuery?sort=registrationDate desc";
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
            var url = "AsyncLightQuery?sort=registrationDate&page=2&pageSize=3";
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
            var url = "AsyncLightQuery?sort=registrationDate desc&page=2&pageSize=3";
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
            var url = "AsyncLightQuery?sort=registrationDate";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.Single(actualResponse.Where(u => u.LastLoginDate != null));
        }

        [Fact]
        public async Task SortByLastLoginDateDescending()
        {
            var url = "AsyncLightQuery?sort=registrationDate desc";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.Single(actualResponse.Where(u => u.LastLoginDate != null));
        }

        [Fact]
        public async Task SortByLastLoginDateWithPagination()
        {
            var url = "AsyncLightQuery?sort=registrationDate&page=2&pageSize=3";
            var actualResponse = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(2, actualResponse.Page);
            Assert.Equal(3, actualResponse.PageSize);
            Assert.Single(actualResponse.Data.Where(u => u.LastLoginDate != null));
        }

        [Fact]
        public async Task SortByLastLoginDateDescendingWithPagination()
        {
            var url = "AsyncLightQuery?sort=registrationDate desc&page=2&pageSize=3";
            var actualResponse = await GetResponse<PaginationResult<User>>(url);
            Assert.Equal(2, actualResponse.Page);
            Assert.Equal(3, actualResponse.PageSize);
            Assert.Empty(actualResponse.Data.Where(u => u.LastLoginDate != null));
        }

        [Fact]
        public async Task DontSortWithoutSortParameter()
        {
            var url = "AsyncLightQuery";
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
            var url = "AsyncLightQuery?sort=firstName";
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
            var url = "AsyncLightQuery?sort=userName&page=1";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageSizeWithSortParam()
        {
            var url = "AsyncLightQuery?sort=userName&pageSize=2";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageAndPageSizeWithSortParam()
        {
            var url = "AsyncLightQuery?sort=userName&page=2&pageSize=3";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageWithoutSortParam()
        {
            var url = "AsyncLightQuery?page=1";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageSizeWithoutSortParam()
        {
            var url = "AsyncLightQuery?pageSize=2";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ReturnsPaginationResultOnRequestingPageAndPageSizeWithoutSortParam()
        {
            var url = "AsyncLightQuery?page=2&pageSize=3";
            var response = await GetResponse<PaginationResult<User>>(url);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task AppliesDefaultSortWithoutClientSortParameter()
        {
            var url = "AsyncLightQueryWithDefaultSort";
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
            var url = "AsyncLightQueryWithDefaultSort?sort=userName";
            var actualResponse = await GetResponse<List<User>>(url);
            for (var i = 1; i < actualResponse.Count; i++)
            {
                var previousValueIsSmaller = actualResponse[i].UserName.CompareTo(actualResponse[i - 1].UserName) > 0;
                Assert.True(previousValueIsSmaller);
            }
        }

        [Fact]
        public async Task CanSortByNestedProperty()
        {
            var url = "AsyncLightQueryWithDefaultSort?sort=favoriteAnimal.name";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.Equal("Joe", actualResponse[0].UserName);
            Assert.Equal("Iris", actualResponse[1].UserName);
            Assert.Equal("Hank", actualResponse[2].UserName);
        }

        [Fact]
        public async Task DoesNotReturnErrorButUnsortedListInCaseOfInvalidPropertyName()
        {
            var url = "AsyncLightQuery?sort=unknownProperty";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.NotEmpty(actualResponse);
        }

        [Fact]
        public async Task DoesNotReturnErrorButUnsortedListInCaseOfInvalidPropertyNameWithRelationalSorting()
        {
            var url = "AsyncLightQuery?sort=unknownProperty.name";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.NotEmpty(actualResponse);
        }

        [Fact]
        public async Task DoesNotReturnErrorButUnsortedListInCaseOfInvalidPropertyNameWithRelationalSortingWithErrorOnSecondLevel()
        {
            var url = "AsyncLightQuery?sort=favoriteAnimal.unknownProperty.name";
            var actualResponse = await GetResponse<List<User>>(url);
            Assert.NotEmpty(actualResponse);
        }

        [Fact]
        public async Task SortByGenderThenByUserName()
        {
            var url = "AsyncLightQuery?sort=gender&thenSort=UserName";
            var actualResponse = await GetResponse<List<User>>(url);

            Assert.Equal(10, actualResponse.Count);
            Assert.Equal("F", actualResponse[0].Gender);
            Assert.Equal("F", actualResponse[1].Gender);
            Assert.Equal("F", actualResponse[2].Gender);
            Assert.Equal("F", actualResponse[3].Gender);
            Assert.Equal("F", actualResponse[4].Gender);
            Assert.Equal("M", actualResponse[5].Gender);
            Assert.Equal("M", actualResponse[6].Gender);
            Assert.Equal("M", actualResponse[7].Gender);
            Assert.Equal("M", actualResponse[8].Gender);
            Assert.Equal("M", actualResponse[9].Gender);

            Assert.Equal("Alice", actualResponse[0].UserName);
            Assert.Equal("Caroline", actualResponse[1].UserName);
            Assert.Equal("Emilia", actualResponse[2].UserName);
            Assert.Equal("Georgia", actualResponse[3].UserName);
            Assert.Equal("Iris", actualResponse[4].UserName);
            Assert.Equal("Bob", actualResponse[5].UserName);
            Assert.Equal("Dave", actualResponse[6].UserName);
            Assert.Equal("Fred", actualResponse[7].UserName);
            Assert.Equal("Hank", actualResponse[8].UserName);
            Assert.Equal("Joe", actualResponse[9].UserName);
        }

        [Fact]
        public async Task SortByGenderThenByUserNameDescending()
        {
            var url = "AsyncLightQuery?sort=gender&thenSort=UserName desc";
            var actualResponse = await GetResponse<List<User>>(url);

            Assert.Equal(10, actualResponse.Count);
            Assert.Equal("F", actualResponse[0].Gender);
            Assert.Equal("F", actualResponse[1].Gender);
            Assert.Equal("F", actualResponse[2].Gender);
            Assert.Equal("F", actualResponse[3].Gender);
            Assert.Equal("F", actualResponse[4].Gender);
            Assert.Equal("M", actualResponse[5].Gender);
            Assert.Equal("M", actualResponse[6].Gender);
            Assert.Equal("M", actualResponse[7].Gender);
            Assert.Equal("M", actualResponse[8].Gender);
            Assert.Equal("M", actualResponse[9].Gender);

            Assert.Equal("Iris", actualResponse[0].UserName);
            Assert.Equal("Georgia", actualResponse[1].UserName);
            Assert.Equal("Emilia", actualResponse[2].UserName);
            Assert.Equal("Caroline", actualResponse[3].UserName);
            Assert.Equal("Alice", actualResponse[4].UserName);
            Assert.Equal("Joe", actualResponse[5].UserName);
            Assert.Equal("Hank", actualResponse[6].UserName);
            Assert.Equal("Fred", actualResponse[7].UserName);
            Assert.Equal("Dave", actualResponse[8].UserName);
            Assert.Equal("Bob", actualResponse[9].UserName);
        }
    }
}
