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
            services.AddMvc();

#if NETCORE3
            services.AddMvc(mvcOptions => mvcOptions.EnableEndpointRouting = false); ;
#else
            services.AddMvc();
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
