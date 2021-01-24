namespace MessageBus.Abstractions.Tests.Unit.Models.Events
{
    public class AircraftTakenOff : IMessage
    {
        public string AircraftId { get; set; }
    }
}
