using MessageBus.Abstractions;
using System;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftTakenOff : IMessage
    {
        public string AircraftId { get; set; }
        public string FlightNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
