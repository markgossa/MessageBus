using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace ServiceBus1
{
    class Program
    {
        static async Task Main()
        {
            var serviceProvider = Startup.Initialize();
            await serviceProvider.GetRequiredService<IMessageBusReceiver>().ConfigureAsync();
            await serviceProvider.GetRequiredService<IMessageBusReceiver>().StartAsync();

            new HostBuilder().Build().Run();
        }
    }
}
