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
            await serviceProvider.GetRequiredService<IMessageBus>().ConfigureAsync();
            await serviceProvider.GetRequiredService<IMessageBus>().StartAsync();

            new HostBuilder().Build().Run();
        }
    }
}
