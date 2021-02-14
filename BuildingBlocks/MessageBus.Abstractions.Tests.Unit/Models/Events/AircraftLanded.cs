namespace MessageBus.Abstractions.Tests.Unit.Models.Events
{
    public class AircraftLanded : IEvent
    {
        public string AircraftId { get; set; }
    }
}
