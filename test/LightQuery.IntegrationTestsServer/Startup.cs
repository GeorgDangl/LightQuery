using LightQuery.NSwag;
using LightQuery.Swashbuckle;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

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
            
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("swagger20", new OpenApiInfo()
                {
                    Description = "swagger20"
                });
                options.OperationFilter<LightQueryOperationFilter>();
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("openapi30", new OpenApiInfo()
                {
                    Description = "openapi30"
                });
                options.OperationFilter<LightQueryOperationFilter>();
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
            
            /*
             * Change the path for swageer json endpoints
             * https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-the-path-for-swagger-json-endpoints
             */
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "swashbuckle/{documentName}.json";
                options.SerializeAsV2 = true;
            });
            
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "swashbuckle/{documentName}.json";
            });

            app.UseMvc();
        }
    }
}
