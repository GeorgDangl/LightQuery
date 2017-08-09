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
    }
}
