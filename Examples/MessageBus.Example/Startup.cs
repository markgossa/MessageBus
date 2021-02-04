using MessageBus.Example.Events;
using MessageBus.Example.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.Example
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public static ServiceProvider Initialize()
        {
            BuildConfiguration();
            return ConfigureServices();
        }

        private static void BuildConfiguration()
            => Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();

        private static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                    Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                    Configuration["ServiceBus:TenantId"]))
                .BuildServiceProvider();
    }
}
