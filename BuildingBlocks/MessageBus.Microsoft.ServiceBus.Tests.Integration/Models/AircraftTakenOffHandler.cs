﻿using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public void Handle(AircraftTakenOff message) => System.Console.WriteLine("Car is red now");
    }
}