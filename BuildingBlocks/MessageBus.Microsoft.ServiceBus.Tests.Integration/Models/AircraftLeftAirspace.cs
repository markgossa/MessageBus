﻿using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftLeftAirspace : IEvent
    {
        public string AircraftIdentifier { get; set; }
    }
}
