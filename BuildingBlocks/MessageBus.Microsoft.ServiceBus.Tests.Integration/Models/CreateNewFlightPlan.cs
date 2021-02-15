using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class CreateNewFlightPlan : ICommand
    {
        public string Destination { get; set; }
        public string Source { get; set; }
    }
}
