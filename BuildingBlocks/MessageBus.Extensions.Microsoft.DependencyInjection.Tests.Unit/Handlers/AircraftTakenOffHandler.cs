using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public void Handle(AircraftTakenOff message) => System.Console.WriteLine("Jeronimo!");
    }
}
