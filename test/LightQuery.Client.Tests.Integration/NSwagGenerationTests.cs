using LightQuery.IntegrationTestsServer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LightQuery.Client.Tests.Integration
{
    public class NSwagGenerationTests
    {
        private readonly HttpClient _client = IntegrationTestServer.GetTestServer().CreateClient();

        [Fact]
        public async Task CanGetSwaggerDocument()
        {
            var swaggerDocResponse = await _client.GetAsync("/nswag/swagger20.json");
            var swaggerDoc = await swaggerDocResponse.Content.ReadAsStringAsync();
            Assert.True(swaggerDoc.Length > 0);
        }

        [Fact]
        public async Task CanGetOpenApiDocument()
        {
            var swaggerDocResponse = await _client.GetAsync("/nswag/openapi30.json");
            var swaggerDoc = await swaggerDocResponse.Content.ReadAsStringAsync();
            Assert.True(swaggerDoc.Length > 0);
        }

        [Fact]
        public async Task GeneratesSortParameterInfo_Swagger()
        {
            var swaggerDoc = await GetSwaggerDocAsync("/nswag/swagger20.json");
            var sortParameter = swaggerDoc.Descendants()
                .OfType<JObject>()
                .FirstOrDefault(d => d["name"]?.Value<string>() == "sort" && d.Parent.Path.EndsWith(".parameters"));
            Assert.NotNull(sortParameter);
        }

        [Fact]
        public async Task GeneratesSortParameterInfo_OpenApi()
        {
            var swaggerDoc = await GetSwaggerDocAsync("/nswag/openapi30.json");
            var sortParameter = swaggerDoc.Descendants()
                .OfType<JObject>()
                .FirstOrDefault(d => d["name"]?.Value<string>() == "sort" && d.Parent.Path.EndsWith(".parameters"));
            Assert.NotNull(sortParameter);
        }

        [Fact]
        public async Task GeneratesThenSortParameterInfo_Swagger()
        {
            var swaggerDoc = await GetSwaggerDocAsync("/nswag/swagger20.json");
            var sortParameter = swaggerDoc.Descendants()
                .OfType<JObject>()
                .FirstOrDefault(d => d["name"]?.Value<string>() == "thenSort" && d.Parent.Path.EndsWith(".parameters"));
            Assert.NotNull(sortParameter);
        }

        [Fact]
        public async Task GeneratesThenSortParameterInfo_OpenApi()
        {
            var swaggerDoc = await GetSwaggerDocAsync("/nswag/openapi30.json");
            var sortParameter = swaggerDoc.Descendants()
                .OfType<JObject>()
                .FirstOrDefault(d => d["name"]?.Value<string>() == "thenSort" && d.Parent.Path.EndsWith(".parameters"));
            Assert.NotNull(sortParameter);
        }

        [Fact]
        public async Task GeneratesPageParameterInfo_Swagger()
        {
            var swaggerDoc = await GetSwaggerDocAsync("/nswag/swagger20.json");
            var sortParameter = swaggerDoc.Descendants()
                .OfType<JObject>()
                .FirstOrDefault(d => d["name"]?.Value<string>() == "page" && d.Parent.Path.EndsWith(".parameters"));
            Assert.NotNull(sortParameter);
        }

        [Fact]
        public async Task GeneratesPageParameterInfo_OpenApi()
        {
            var swaggerDoc = await GetSwaggerDocAsync("/nswag/openapi30.json");
            var sortParameter = swaggerDoc.Descendants()
                .OfType<JObject>()
                .FirstOrDefault(d => d["name"]?.Value<string>() == "page" && d.Parent.Path.EndsWith(".parameters"));
            Assert.NotNull(sortParameter);
        }

        [Fact]
        public async Task GeneratesPageSizeParameterInfo_Swagger()
        {
            var swaggerDoc = await GetSwaggerDocAsync("/nswag/swagger20.json");
            var sortParameter = swaggerDoc.Descendants()
                .OfType<JObject>()
                .FirstOrDefault(d => d["name"]?.Value<string>() == "pageSize" && d.Parent.Path.EndsWith(".parameters"));
            Assert.NotNull(sortParameter);
        }

        [Fact]
        public async Task GeneratesPageSizeParameterInfo_OpenApi()
        {
            var swaggerDoc = await GetSwaggerDocAsync("/nswag/openapi30.json");
            var sortParameter = swaggerDoc.Descendants()
                .OfType<JObject>()
                .FirstOrDefault(d => d["name"]?.Value<string>() == "pageSize" && d.Parent.Path.EndsWith(".parameters"));
            Assert.NotNull(sortParameter);
        }

        private async Task<JObject> GetSwaggerDocAsync(string specUrl)
        {
            var swaggerDocResponse = await _client.GetAsync(specUrl);
            var swaggerDoc = await swaggerDocResponse.Content.ReadAsStringAsync();
            return JObject.Parse(swaggerDoc);
        }
    }
}
