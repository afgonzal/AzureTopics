using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.Sales
{
    class Program
    {
        static void Main(string[] args)
        {

            var config = LoadConfiguration();
            using (ServiceProvider svcProvider = ConfigureDependencyInjection(config))
            {
                var app = svcProvider.GetService<ContosoApp>();
                app.Run();
            }
        }

        private static ServiceProvider ConfigureDependencyInjection(IConfiguration config)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            Sales.Business.Startup.ConfigureServices(services,config);
            services.AddTransient<ContosoApp>();
            return services.BuildServiceProvider();
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true,
                    reloadOnChange: true);
            //.AddUserSecrets<Program>();

            return builder.Build();
        }
    }
}
