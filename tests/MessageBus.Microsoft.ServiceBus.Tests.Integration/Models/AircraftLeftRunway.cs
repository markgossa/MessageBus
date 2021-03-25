using MessageBus.Abstractions.Messages;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftLeftRunway : IEvent
    {
        public string RunwayId { get; set; }
    }
}
