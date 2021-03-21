using MessageBus.Abstractions.Messages;

namespace MessageBus.HostedService.Example.Events
{
    public class AircraftLeftAirspace : IEvent
    {
        public string Airspace { get; set; }
    }
}