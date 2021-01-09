using EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServiceBus1.Events;
using Xunit;

namespace ServiceBus1.Tests.Unit
{
    public class StartupTests
    {
        [Fact]
        public void RegistersAircraftTakenOffHandler()
        {
            var serviceProvider = Startup.ConfigureServices();

            Assert.NotNull(serviceProvider.GetRequiredService<IHandleMessages<AircraftTakenOff>>());
        }
    }
}
