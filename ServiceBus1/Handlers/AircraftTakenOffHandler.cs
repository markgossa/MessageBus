using ServiceBus1.EventBus;
using ServiceBus1.Events;
using System.Diagnostics;

namespace ServiceBus1.Handlers
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public void Handle(AircraftTakenOff aircraftTakenOff) 
            => Debug.WriteLine(aircraftTakenOff.AircraftId);
    }
}
