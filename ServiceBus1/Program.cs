using Microsoft.Extensions.Hosting;

namespace ServiceBus1
{
    class Program
    {
        static void Main()
        {
            Startup.Initialize();
            new HostBuilder().Build().Run();
        }
    }
}
