using System.Net.Http;
using System.Threading.Tasks;
using LightQuery.IntegrationTestsServer;
using Newtonsoft.Json;
using Xunit;

namespace LightQuery.Tests.Integration.ControllerTests
{
    public abstract class ControllerTestBase
    {
        private HttpClient _Client = IntegrationTestServer.GetTestServer().CreateClient();

        protected async Task<T> GetResponse<T>(string url)
        {
            var response = await _Client.GetAsync(url);
            Assert.True(response.IsSuccessStatusCode, "The HttpResponse indicates a non-success status code");
            var responseContent = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<T>(responseContent);
            return deserializedResponse;
        }
    }
}
