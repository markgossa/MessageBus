using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public void Handle(AircraftLanded message) => System.Console.WriteLine("Welcome!");
    }
}
