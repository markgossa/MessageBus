namespace MessageBus.Abstractions.Tests.Unit.Models.Events
{
    public class AircraftTakenOff : IEvent
    {
        public string AicraftId { get; set; }
    }
}
