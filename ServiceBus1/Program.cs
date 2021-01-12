using MessageBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBus1.Events;
using System;
using System.Threading.Tasks;

namespace ServiceBus1
{
    class Program
    {
        private static IConfiguration _config;

        static async Task Main()
        {
            var services = Startup.Initialize();
            var serviceProvider = services.BuildServiceProvider();

            var handler = serviceProvider.GetRequiredService<IHandleMessages<AircraftTakenOff>>();

            _config = Startup.Configuration;
            var messageBus = new MessageBus.Microsoft.ServiceBus.MessageBus(GetConfigValue("ServiceBus:ConnectionString"), 
                GetConfigValue("ServiceBus:Topic"), GetConfigValue("ServiceBus:Subscription"));
            await messageBus.StartAsync(services);

            new HostBuilder().Build().Run();
        }

        private static string GetConfigValue(string settingName) 
            => _config.GetSection(settingName).Value;
    }
}
