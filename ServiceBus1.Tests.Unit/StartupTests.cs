using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServiceBus1.Events;
using Xunit;

namespace ServiceBus1.Tests.Unit
{
    public class StartupTests
    {
        //[Fact]
        //public void RegistersAircraftTakenOffHandler()
        //{
        //    var serviceProvider = Startup.Initialize();

        //    Assert.NotNull(serviceProvider.GetRequiredService<IHandleMessages<AircraftTakenOff>>());
        //}
        
        [Fact]
        public void AddsConfiguration()
        {
            Startup.Initialize();

            Assert.NotNull(Startup.Configuration);
            Assert.Equal("topic1", Startup.Configuration.GetSection("ServiceBus:Topic").Value);
        }
    }
}
