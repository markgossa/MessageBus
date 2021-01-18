using MessageBus.Abstractions;
using System;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftLanded : IMessage
    {
        public string AircraftId { get; init; }
        public string FlightNumber { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
