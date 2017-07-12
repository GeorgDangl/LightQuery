using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace LightQuery.Tests
{
    public class QueryParserTests
    {
        private IQueryCollection _query;
        private QueryOptions _result;

        private void ParseQuery()
        {
            _result = QueryParser.GetQueryOptions(_query);
        }

        [Fact]
        public void ArgumentNullExceptionOnNullQuery()
        {
            Assert.Throws<ArgumentNullException>("query", () => QueryParser.GetQueryOptions(null));
        }

        [Fact]
        public void NullSortPropertyNameWhenNoSortParameter()
        {
            _query = QueryCollection.Empty;
            ParseQuery();
            Assert.Null(_result.SortPropertyName);
        }

        [Fact]
        public void NullSortPropertyNameWhenNoSortParameterButOthers()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {{"makeSunnyWeather", new Microsoft.Extensions.Primitives.StringValues("true")}});
            ParseQuery();
            Assert.Null(_result.SortPropertyName);
        }

        [Fact]
        public void NullSortPropertyNameWhenThreeSpaceSeparatedGroups()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {{"sort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " asc invalid") }});
            ParseQuery();
            Assert.Null(_result.SortPropertyName);
        }

        [Fact]
        public void ParseSortPropertyName()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {{"sort", new Microsoft.Extensions.Primitives.StringValues(propertyName)}});
            ParseQuery();
            var expectedPropertyName = "Email";
            Assert.Equal(expectedPropertyName, _result.SortPropertyName);
        }

        [Fact]
        public void CamelizeSortPropertyName()
        {
            var propertyName = "email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            var expectedPropertyName = "Email";
            Assert.Equal(expectedPropertyName, _result.SortPropertyName);
        }

        [Fact]
        public void CamelizeLongerSortPropertyName()
        {
            var propertyName = "userName";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            var expectedPropertyName = "UserName";
            Assert.Equal(expectedPropertyName, _result.SortPropertyName);
        }

        [Fact]
        public void IsAscendingForRegularQuery()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            Assert.False(_result.IsDescending);
        }

        [Fact]
        public void IsAscendingWithCorrectParameter()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " asc") } });
            ParseQuery();
            Assert.False(_result.IsDescending);
        }

        [Fact]
        public void IsDescendingWithCorrectParameter()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " desc") } });
            ParseQuery();
            Assert.True(_result.IsDescending);
        }
    }
}
