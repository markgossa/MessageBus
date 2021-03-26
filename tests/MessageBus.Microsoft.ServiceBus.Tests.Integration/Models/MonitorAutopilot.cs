using MessageBus.Abstractions.Messages;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class MonitorAutopilot : ICommand
    {
        public string AutopilotIdentifider { get; set; }
    }
}
