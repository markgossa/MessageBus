using MessageBus.Abstractions.Tests.Unit.Models.Events;

namespace MessageBus.Abstractions.Tests.Unit.Handlers
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public void Handle(AircraftTakenOff message) => System.Console.WriteLine("Jeronimo!");
    }
}
