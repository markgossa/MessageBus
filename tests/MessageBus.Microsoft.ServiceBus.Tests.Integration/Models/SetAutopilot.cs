using MessageBus.Abstractions.Messages;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class SetAutopilot : ICommand
    {
        public string AutopilotId { get; set; }
    }
}
