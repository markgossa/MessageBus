using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class SetAutopilot : ICommand
    {
        public string AutopilotId { get; set; }
    }
}
