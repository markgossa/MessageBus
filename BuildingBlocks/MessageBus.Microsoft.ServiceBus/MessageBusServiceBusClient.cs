using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Utilities;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus
{
    public class MessageBusServiceBusClient : IMessageBusClient
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;

        public MessageBusServiceBusClient(string connectionString, string topic, string subscription)
        {
            _serviceBusProcessor = new ServiceBusClient(connectionString).CreateProcessor(topic, subscription);
        }
        
        public MessageBusServiceBusClient(string hostname, string topic, string subscription, 
            string tenantId = null)
        {
            _serviceBusProcessor = new ServiceBusClient(hostname, new ServiceBusTokenProvider(tenantId))
                .CreateProcessor(topic, subscription);
        }

        public void AddErrorMessageHandler(Func<EventArgs, Task> errorMessageHandler)
        {
            _serviceBusProcessor.ProcessErrorAsync += errorMessageHandler;
        }

        public void AddMessageHandler(Func<EventArgs, Task> messageHandler) 
            => _serviceBusProcessor.ProcessMessageAsync += messageHandler;

        public async Task StartAsync() => await _serviceBusProcessor.StartProcessingAsync();
    }
}
