using MessageBus.Abstractions;
using ServiceBus1.Events;
using System;

namespace ServiceBus1.Handlers
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public void Handle(AircraftTakenOff aircraftTakenOff)
            => Console.WriteLine(aircraftTakenOff.AircraftId);
    }
}
