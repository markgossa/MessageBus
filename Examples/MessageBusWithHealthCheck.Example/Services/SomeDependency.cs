using System;
using System.Threading.Tasks;

namespace MessageBusWithHealthCheck.Example.Services
{
    public class SomeDependency : IDependency
    {
        public async Task WriteLineAfterDelay(string text)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            Console.WriteLine(text);
        }
    }
}
