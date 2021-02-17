using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftLeftRunway : IEvent
    {
        public string RunwayId { get; set; }
    }
}
