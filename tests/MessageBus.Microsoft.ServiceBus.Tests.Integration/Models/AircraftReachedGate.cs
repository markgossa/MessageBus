using MessageBus.Abstractions.Messages;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftReachedGate : IEvent
    {
        public string AirlineId { get; set; }
    }
}
