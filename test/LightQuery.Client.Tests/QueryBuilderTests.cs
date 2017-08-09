using System;
using System.Collections.Generic;
using Xunit;

namespace LightQuery.Client.Tests
{
    public class QueryBuilderTests
    {
        [Fact]
        public void ArgumentExceptionOnZeroPage()
        {
            Assert.Throws<ArgumentException>(() => QueryBuilder.Build(page: 0));
        }

        [Fact]
        public void ArgumentExceptionOnNegativePage()
        {
            Assert.Throws<ArgumentException>(() => QueryBuilder.Build(page: -4));
        }

        [Fact]
        public void ArgumentExceptionOnZeroPageSize()
        {
            Assert.Throws<ArgumentException>(() => QueryBuilder.Build(pageSize: 0));
        }

        [Fact]
        public void ArgumentExceptionOnNegativePageSize()
        {
            Assert.Throws<ArgumentException>(() => QueryBuilder.Build(pageSize: -4));
        }

        [Fact]
        public void BuildsWithDefaultPageAndPageSize()
        {
            var expectedQueryString = "?page=1&pageSize=20";
            var actualQueryString = QueryBuilder.Build();
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void AddsSortParameter()
        {
            var expectedQueryString = "?page=1&pageSize=20&sort=username%20asc";
            var actualQueryString = QueryBuilder.Build(sortParam: "username");
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void AddsSortDescendingParameter()
        {
            var expectedQueryString = "?page=1&pageSize=20&sort=username%20desc";
            var actualQueryString = QueryBuilder.Build(sortParam: "username", sortDescending: true);
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void AddsCustomArgument()
        {
            var expectedQueryString = "?page=1&pageSize=20&isInRole=admin";
            var actualQueryString = QueryBuilder.Build(additionalParameters: new Dictionary<string, string> { { "isInRole", "admin" } });
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void AddsMultipleCustomArguments()
        {
            var expectedQueryString = "?page=1&pageSize=20&isInRole=admin&accountLocked=false";
            var actualQueryString = QueryBuilder.Build(additionalParameters: new Dictionary<string, string> { { "isInRole", "admin" }, { "accountLocked", "false" } });
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void AddsOnlyCustomParameterIfValueIsNull()
        {
            var expectedQueryString = "?page=1&pageSize=20&isAdmin";
            var actualQueryString = QueryBuilder.Build(additionalParameters: new Dictionary<string, string> { { "isAdmin", null } });
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void AddsOnlyCustomParameterIfValueIsNullWithMultipleValues()
        {
            var expectedQueryString = "?page=1&pageSize=20&isAdmin&accountLocked=false";
            var actualQueryString = QueryBuilder.Build(additionalParameters: new Dictionary<string, string> { { "isAdmin", null }, { "accountLocked", "false" } });
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void UrlEscapesSortParameter()
        {
            var expectedQueryString = "?page=1&pageSize=20&sort=user%20name%20asc";
            var actualQueryString = QueryBuilder.Build(sortParam: "user name");
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void UrlEscapesCustomParameter()
        {
            var expectedQueryString = "?page=1&pageSize=20&is%20In%20Role=admin";
            var actualQueryString = QueryBuilder.Build(additionalParameters: new Dictionary<string, string> { { "is In Role", "admin" } });
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void UrlEscapesCustomParameterValue()
        {
            var expectedQueryString = "?page=1&pageSize=20&isInRole=super%20admin";
            var actualQueryString = QueryBuilder.Build(additionalParameters: new Dictionary<string, string> { { "isInRole", "super admin" } });
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void UrlEscapesCustomParameterWithoutValue()
        {
            var expectedQueryString = "?page=1&pageSize=20&is%20Admin";
            var actualQueryString = QueryBuilder.Build(additionalParameters: new Dictionary<string, string> { { "is Admin", null } });
            Assert.Equal(expectedQueryString, actualQueryString);
        }

        [Fact]
        public void BuildCompleteQuery()
        {
            var expectedQueryString = "?page=13&pageSize=25&sort=email%20desc&filter=bob&isAdmin";
            var actualQueryString = QueryBuilder.Build(page: 13, pageSize: 25, sortParam: "email", sortDescending: true, additionalParameters: new Dictionary<string, string> {{"filter", "bob"}, {"isAdmin", null}});
            Assert.Equal(expectedQueryString, actualQueryString);
        }
    }
}
