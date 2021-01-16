using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public void Handle(AircraftLanded message) => System.Console.WriteLine("Car is clean");
    }
}
