using MessageBus.Abstractions.Messages;
using System;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit.Models
{
    public class AircraftTakenOff : IEvent
    {
        public string AircraftId { get; set; }
        public string FlightNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
