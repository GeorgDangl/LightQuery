using System;
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
    }
}
