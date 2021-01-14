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
            _config = Startup.Configuration;
            var messageBus = new MessageBus.Microsoft.ServiceBus.MessageBusServiceBusAdmin(GetConfigValue("ServiceBus:ConnectionString"), 
                GetConfigValue("ServiceBus:Topic"), GetConfigValue("ServiceBus:Subscription"));
            //await messageBus.ConfigureAsync(services);

            new HostBuilder().Build().Run();
        }

        private static string GetConfigValue(string settingName) 
            => _config.GetSection(settingName).Value;
    }
}
