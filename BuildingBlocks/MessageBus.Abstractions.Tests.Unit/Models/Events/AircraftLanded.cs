namespace MessageBus.Abstractions.Tests.Unit.Models.Events
{
    [MessageVersion(1)]
    public class AircraftLanded : IEvent
    {
        public string AircraftId { get; set; }
    }
}
