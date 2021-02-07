#nullable disable

namespace MessageBus.Microsoft.ServiceBus
{
    public class AzureServiceBusAdminClientOptions
    {
        public string MessageTypePropertyName { get; set; }
        public string MessageVersionPropertyName { get; set; }
    }
}