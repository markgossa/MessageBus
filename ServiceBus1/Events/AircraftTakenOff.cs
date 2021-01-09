using EventBus.Abstractions;

namespace ServiceBus1.Events
{
    public class AircraftTakenOff : IEvent
    {
        public string AircraftId { get; set; }
    }
}