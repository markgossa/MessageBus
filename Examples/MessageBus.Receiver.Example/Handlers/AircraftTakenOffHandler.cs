using MessageBus.Abstractions;
using ServiceBus1.Events;
using System;
using System.Threading.Tasks;

namespace ServiceBus1.Handlers
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public async Task HandleAsync(AircraftTakenOff aircraftTakenOff)
            => Console.WriteLine(aircraftTakenOff.AircraftId);
    }
}
