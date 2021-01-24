namespace MessageBus.Abstractions.Tests.Unit.Models.Events
{
    public class AircraftLanded : IMessage
    {
        public string AircraftId { get; set; }
    }
}
