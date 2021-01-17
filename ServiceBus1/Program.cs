using MessageBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBus1.Events;
using System.Threading.Tasks;

namespace ServiceBus1
{
    class Program
    {
        private static IConfiguration _config;

        static async Task Main()
        {
            var services = Startup.Initialize();

            new HostBuilder().Build().Run();
        }
    }
}
