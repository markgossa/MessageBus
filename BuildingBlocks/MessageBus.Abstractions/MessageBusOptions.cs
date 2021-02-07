#nullable disable
namespace MessageBus.Abstractions
{
    public class MessageBusOptions
    {
        public string MessageTypePropertyName { get; set; }
        public string MessageVersionPropertyName { get; set; }
    }
}
