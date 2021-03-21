using MessageBus.Abstractions.Messages;

namespace MessageBus.Abstractions.Tests.Unit.Models.Commands.V2
{
    [MessageVersion(2)]
    public class CreateNewFlightPlan : ICommand
    {
        public string Source { get; set; }
        public string Destination { get; set; }
    }
}
