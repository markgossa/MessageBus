namespace MessageBus.Abstractions.Tests.Unit.Models.Events.V2
{
    [MessageVersion(2)]
    public class CreateNewFlightPlan : ICommand
    {
        public string Source { get; set; }
        public string Destination { get; set; }
    }
}
