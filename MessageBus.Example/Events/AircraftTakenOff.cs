using MessageBus.Abstractions;
using System;

namespace ServiceBus1.Events
{
    public class AircraftTakenOff : IMessage
    {
        public string AircraftId { get; set; }
        public string FlightNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}