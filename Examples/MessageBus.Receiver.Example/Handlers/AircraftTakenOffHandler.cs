using MessageBus.Abstractions;
using ServiceBus1.Events;
using System;
using System.Threading.Tasks;

namespace ServiceBus1.Handlers
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public async Task HandleAsync(AircraftTakenOff aircraftTakenOff)
        {
            //await Task.Delay(TimeSpan.FromMilliseconds(5));
            Console.WriteLine($"{nameof(AircraftTakenOff)} message received with AircraftId: {aircraftTakenOff.AircraftId}");
        }
    }
}
