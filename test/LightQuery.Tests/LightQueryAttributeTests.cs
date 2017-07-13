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
    }
}
