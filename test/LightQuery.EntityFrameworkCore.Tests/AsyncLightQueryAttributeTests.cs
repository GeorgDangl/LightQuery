using System;
using System.Threading.Tasks;
using Xunit;

namespace LightQuery.EntityFrameworkCore.Tests
{
    public class AsyncLightQueryAttributeTests
    {
        [Fact]
        public async Task ArgumentNullExceptionOnNullContext()
        {
            await Assert.ThrowsAsync<ArgumentNullException>("context", async () => await new AsyncLightQueryAttribute().OnResultExecutionAsync(null, null));
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
                Assert.Throws<ArgumentException>("defaultSort", () => new AsyncLightQueryAttribute(defaultSort: defaultSort));
            }
            else
            {
                var attribute = new AsyncLightQueryAttribute(defaultSort: defaultSort);
                Assert.NotNull(attribute);
            }
        }
    }
}
