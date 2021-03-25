using MessageBus.Abstractions.Messages;
using System;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftLanded : IEvent
    {
        public string AircraftId { get; init; }
        public string FlightNumber { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
