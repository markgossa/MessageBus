namespace MessageBus.Microsoft.ServiceBus
{
    public class AzureServiceBusAdminClientOptions
    {
        public string MessageTypePropertyName { get; init; }
        public string MessageVersionPropertyName { get; init; }
    }
}