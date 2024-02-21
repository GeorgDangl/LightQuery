﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Xunit;
#if NET6
using Microsoft.AspNetCore.Http.Internal;
#endif

namespace LightQuery.Shared.Tests
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
            Assert.Null(_result.Sort?.PropertyName);
        }

        [Fact]
        public void NullSortPropertyNameWhenNoSortParameterButOthers()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {{"makeSunnyWeather", new Microsoft.Extensions.Primitives.StringValues("true")}});
            ParseQuery();
            Assert.Null(_result.Sort?.PropertyName);
        }

        [Fact]
        public void NullSortPropertyNameWhenThreeSpaceSeparatedGroups()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {{"sort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " asc invalid") }});
            ParseQuery();
            Assert.Null(_result.Sort?.PropertyName);
        }

        [Fact]
        public void ParseSortPropertyName()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {{"sort", new Microsoft.Extensions.Primitives.StringValues(propertyName)}});
            ParseQuery();
            var expectedPropertyName = "Email";
            Assert.Equal(expectedPropertyName, _result.Sort.PropertyName);
        }

        [Fact]
        public void DoNotCamelizeSortPropertyName()
        {
            // Camelization is done when the query is applied, not when the options are parsed
            var propertyName = "email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            var expectedPropertyName = "email";
            Assert.Equal(expectedPropertyName, _result.Sort.PropertyName);
        }

        [Fact]
        public void DoNotCamelizeLongerSortPropertyName()
        {
            // Camelization is done when the query is applied, not when the options are parsed
            var propertyName = "userName";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            var expectedPropertyName = "userName";
            Assert.Equal(expectedPropertyName, _result.Sort.PropertyName);
        }

        [Fact]
        public void IsAscendingForRegularQuery()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            Assert.False(_result.Sort.IsDescending);
        }

        [Fact]
        public void IsAscendingWithCorrectParameter()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " asc") } });
            ParseQuery();
            Assert.False(_result.Sort.IsDescending);
        }

        [Fact]
        public void IsDescendingWithCorrectParameter()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "sort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " desc") } });
            ParseQuery();
            Assert.True(_result.Sort.IsDescending);
        }

        [Fact]
        public void NullSortPropertyNameWhenNoSortParameter_ForThenSort()
        {
            _query = QueryCollection.Empty;
            ParseQuery();
            Assert.Null(_result.ThenSort?.PropertyName);
        }

        [Fact]
        public void NullSortPropertyNameWhenNoSortParameterButOthers_ForThenSort()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "makeSunnyWeather", new Microsoft.Extensions.Primitives.StringValues("true") } });
            ParseQuery();
            Assert.Null(_result.ThenSort?.PropertyName);
        }

        [Fact]
        public void NullSortPropertyNameWhenThreeSpaceSeparatedGroups_ForThenSort()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "thenSort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " asc invalid") } });
            ParseQuery();
            Assert.Null(_result.ThenSort?.PropertyName);
        }

        [Fact]
        public void ParseSortPropertyName_ForThenSort()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "thenSort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            var expectedPropertyName = "Email";
            Assert.Equal(expectedPropertyName, _result.ThenSort.PropertyName);
        }

        [Fact]
        public void DoNotCamelizeSortPropertyName_ForThenSort()
        {
            // Camelization is done when the query is applied, not when the options are parsed
            var propertyName = "email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "thenSort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            var expectedPropertyName = "email";
            Assert.Equal(expectedPropertyName, _result.ThenSort.PropertyName);
        }

        [Fact]
        public void DoNotCamelizeLongerSortPropertyName_ForThenSort()
        {
            // Camelization is done when the query is applied, not when the options are parsed
            var propertyName = "userName";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "thenSort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            var expectedPropertyName = "userName";
            Assert.Equal(expectedPropertyName, _result.ThenSort.PropertyName);
        }

        [Fact]
        public void IsAscendingForRegularQuery_ForThenSort()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "thenSort", new Microsoft.Extensions.Primitives.StringValues(propertyName) } });
            ParseQuery();
            Assert.False(_result.ThenSort.IsDescending);
        }

        [Fact]
        public void IsAscendingWithCorrectParameter_ForThenSort()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "thenSort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " asc") } });
            ParseQuery();
            Assert.False(_result.ThenSort.IsDescending);
        }

        [Fact]
        public void IsDescendingWithCorrectParameter_ForThenSort()
        {
            var propertyName = "Email";
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "thenSort", new Microsoft.Extensions.Primitives.StringValues(propertyName + " desc") } });
            ParseQuery();
            Assert.True(_result.ThenSort.IsDescending);
        }

        [Fact]
        public void ParsePage()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "page", new Microsoft.Extensions.Primitives.StringValues("12") } });
            ParseQuery();
            Assert.Equal(12, _result.Page);
        }

        [Fact]
        public void UseFirstPageForZeroPage()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "page", new Microsoft.Extensions.Primitives.StringValues("0") } });
            ParseQuery();
            Assert.Equal(1, _result.Page);
        }

        [Fact]
        public void UseFirstPageForNegativePage()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "page", new Microsoft.Extensions.Primitives.StringValues("-2") } });
            ParseQuery();
            Assert.Equal(1, _result.Page);
        }

        [Fact]
        public void UseFirstPageForNaNPage()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "page", new Microsoft.Extensions.Primitives.StringValues("second") } });
            ParseQuery();
            Assert.Equal(1, _result.Page);
        }

        [Fact]
        public void ParsePageSize()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "pageSize", new Microsoft.Extensions.Primitives.StringValues("20") } });
            ParseQuery();
            Assert.Equal(20, _result.PageSize);
        }

        [Fact]
        public void ParsePageSizeBiggerThanDefault()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "pageSize", new Microsoft.Extensions.Primitives.StringValues("200") } });
            ParseQuery();
            Assert.Equal(200, _result.PageSize);
        }

        [Fact]
        public void UseDefaultPageSizeForZeroPageSize()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "pageSize", new Microsoft.Extensions.Primitives.StringValues("0") } });
            ParseQuery();
            Assert.Equal(50, _result.PageSize);
        }

        [Fact]
        public void UseDefaultPageSizeForNegativePageSize()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "pageSize", new Microsoft.Extensions.Primitives.StringValues("-3") } });
            ParseQuery();
            Assert.Equal(50, _result.PageSize);
        }

        [Fact]
        public void UseDefaultPageSizeForNaNPageSize()
        {
            _query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "pageSize", new Microsoft.Extensions.Primitives.StringValues("twenty") } });
            ParseQuery();
            Assert.Equal(50, _result.PageSize);
        }
    }
}
