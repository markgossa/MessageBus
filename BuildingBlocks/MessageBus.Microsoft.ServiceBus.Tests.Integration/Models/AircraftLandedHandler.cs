using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public void Handle(AircraftLanded message) => System.Console.WriteLine("Car is clean");
    }
}
