using MessageBus.Abstractions;

namespace ServiceBus1.Events
{
    public class AircraftTakenOff : IMessage
    {
        public string AircraftId { get; set; }
    }
}