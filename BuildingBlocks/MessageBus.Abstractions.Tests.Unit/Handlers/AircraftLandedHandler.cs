using MessageBus.Abstractions.Tests.Unit.Models.Events;

namespace MessageBus.Abstractions.Tests.Unit.Handlers
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public void Handle(AircraftLanded message) => System.Console.WriteLine("Welcome!");
    }
}
