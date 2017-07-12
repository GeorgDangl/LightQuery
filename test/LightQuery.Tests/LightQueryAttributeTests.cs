using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunit;

namespace LightQuery.Tests
{
    public class LightQueryAttributeTests
    {
        private readonly LightQueryAttribute _attribute = new LightQueryAttribute();

        [Fact]
        public void ArgumentNullExceptionOnNullContext()
        {
            Assert.Throws<ArgumentNullException>("context", () => new LightQueryAttribute().OnResultExecuting(null));
        }

        [Fact]
        public void DoNothingWhenNoQueryParameters()
        {
            var context = GetMockedContext();
            Assert.False(context.HttpContext.Request.Query.Any());
            throw new NotImplementedException();
        }

        [Fact]
        public void DoNothingWhenNullQueryParameters()
        {
            var context = GetMockedContext();
            context.HttpContext.Request.Query = null;
            throw new NotImplementedException();
        }

        [Fact]
        public void DoNothingWhenInvalidPropertyName()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void DoNothingWhenNoQueryableContent()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void CallQueryableProcessorOnValidQuery()
        {
            throw new NotImplementedException();
        }

        private ResultExecutingContext GetMockedContext()
        {
            throw new NotImplementedException();
        }
    }
}
