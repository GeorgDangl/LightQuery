using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LightQuery.IntegrationTestsServer
{
    public class IntegrationTestServer
    {
        private static IHost _testHost;
        private static readonly object _initializationLock = new object();

        public static TestServer GetTestServer()
        {
            if (_testHost == null)
            {
                InitializeTestServer();
            }
            return _testHost.GetTestServer();
        }

        public static LightQueryContext GetNewContext()
        {
            using (var serviceScope = GetTestServer().Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                var context = serviceProvider.GetRequiredService<LightQueryContext>();
                return context;
            }
        }

        private static void InitializeTestServer()
        {
            lock (_initializationLock)
            {
                if (_testHost != null)
                {
                    return;
                }
                var host = new HostBuilder()
                    .ConfigureWebHost(webHostBuilder => webHostBuilder
                        .UseTestServer()
                        .ConfigureLogging(logging => logging.AddDebug())
                        .UseStartup<Startup>())
                    .Start();
                InitializeDatabase(host.GetTestServer()).GetAwaiter().GetResult();
                _testHost = host;
            }
        }

        private static async Task InitializeDatabase(TestServer testServer)
        {
            using (var serviceScope = testServer.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                var context = serviceProvider.GetRequiredService<LightQueryContext>();
                var initializer = new DatabaseInitializer(context);
                await initializer.InitializeDatabase();
            }
        }
    }
}
