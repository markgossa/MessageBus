using EventBus.Extensions.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceBus1.Events;
using ServiceBus1.Handlers;

namespace ServiceBus1
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }


        public static ServiceProvider Initialize()
        {
            BuildConfiguration();
            return ConfigureServices();
        }

        private static ServiceProvider ConfigureServices() 
            => new ServiceCollection()
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .BuildServiceProvider();

        private static void BuildConfiguration() 
            => Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();
    }
}
