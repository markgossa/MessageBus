using MessageBus.Abstractions;

namespace MessageBus.Example.Events
{
    public class AircraftTakenOff : IEvent
    {
        public string AircraftId { get; set; }
        public string SourceAirport { get; set; }
    }
}