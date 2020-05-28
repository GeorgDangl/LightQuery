using LightQuery.NSwag;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LightQuery.IntegrationTestsServer
{
    public class Startup
    {
        private static readonly string _inMemoryDatabaseName = System.Guid.NewGuid().ToString();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LightQueryContext>(options => options.UseInMemoryDatabase(_inMemoryDatabaseName));

            services.AddSwaggerDocument(nSwagConfig =>
            {
                nSwagConfig.DocumentName = "swagger20";
                nSwagConfig.OperationProcessors.Add(new LightQueryOperationsProcessor());
            });
            services.AddOpenApiDocument(nSwagConfig =>
            {
                nSwagConfig.DocumentName = "openapi30";
                nSwagConfig.OperationProcessors.Add(new LightQueryOperationsProcessor());
            });

#if NETCORE3
            services.AddMvc(mvcOptions => mvcOptions.EnableEndpointRouting = false); ;
#else
            services.AddMvc();
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseOpenApi(openApiConfig =>
            {
                openApiConfig.DocumentName = "swagger20";
                openApiConfig.Path = "/nswag/swagger20.json";
            });
            app.UseOpenApi(openApiConfig =>
            {
                openApiConfig.DocumentName = "openapi30";
                openApiConfig.Path = "/nswag/openapi30.json";
            });

            app.UseMvc();
        }
    }
}
