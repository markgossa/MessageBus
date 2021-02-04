using MessageBus.Abstractions;

namespace MessageBusWithHealthCheck.Example.Events
{
    public class AircraftTakenOff : IEvent
    {
        public string AircraftId { get; set; }
        public string SourceAirport { get; set; }
    }
}