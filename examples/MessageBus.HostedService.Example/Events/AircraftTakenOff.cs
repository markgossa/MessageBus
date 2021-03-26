using MessageBus.Abstractions.Messages;

namespace MessageBus.HostedService.Example.Events
{
    public class AircraftTakenOff : IEvent
    {
        public string AircraftId { get; set; }
        public string SourceAirport { get; set; }
    }
}