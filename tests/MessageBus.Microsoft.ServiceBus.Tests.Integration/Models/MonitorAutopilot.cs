using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class MonitorAutopilot : ICommand
    {
        public string AutopilotIdentifider { get; set; }
    }
}
