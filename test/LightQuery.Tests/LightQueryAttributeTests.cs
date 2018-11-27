using System;
using Xunit;

namespace LightQuery.Tests
{
    public class LightQueryAttributeTests
    {
        [Fact]
        public void ArgumentNullExceptionOnNullContext()
        {
            Assert.Throws<ArgumentNullException>("context", () => new LightQueryAttribute().OnResultExecuting(null));
        }

        [Theory]
        [InlineData("prop asc", false)]
        [InlineData("prop ASC", false)]
        [InlineData("prop Asc", false)]
        [InlineData("prop desc", false)]
        [InlineData("prop DESC", false)]
        [InlineData("prop Desc", false)]
        [InlineData("prop funky", true)]
        [InlineData("prop", false)] // interpreted as asc
        [InlineData("prop asc asc", true)]
        [InlineData("prop asc funky", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void HandlesDefaultSortParameter(string defaultSort, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentException>("defaultSort", () => new LightQueryAttribute(defaultSort: defaultSort));
            }
            else
            {
                var attribute = new LightQueryAttribute(defaultSort: defaultSort);
                Assert.NotNull(attribute);
            }
        }
    }
}
