using MessageBus.Abstractions.Messages;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftLeftAirspace : IEvent
    {
        public string AircraftIdentifier { get; set; }
    }
}
