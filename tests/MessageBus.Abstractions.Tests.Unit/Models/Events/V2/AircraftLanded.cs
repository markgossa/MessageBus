using MessageBus.Abstractions.Messages;
using System;

namespace MessageBus.Abstractions.Tests.Unit.Models.Events.V2
{
    [MessageVersion(2)]
    public class AircraftLanded : IEvent
    {
        public string AircraftId { get; init; }
        public string FlightNumber { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
