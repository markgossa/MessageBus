using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class StartEngines : ICommand
    {
        public string EngineId { get; set; }
    }
}
