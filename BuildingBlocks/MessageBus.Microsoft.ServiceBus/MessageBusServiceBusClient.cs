using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus
{
    public class MessageBusServiceBusClient : IMessageBusClient
    {
        private readonly string _connectionString;
        private readonly string _topic;
        private readonly string _subscription;
        private readonly ServiceBusProcessor _serviceBusProcessor;

        public MessageBusServiceBusClient(string connectionString, string topic, string subscription)
        {
            _connectionString = connectionString;
            _topic = topic;
            _subscription = subscription;
            _serviceBusProcessor = new ServiceBusClient(_connectionString).CreateProcessor(_topic, _subscription);
        }

        public void AddErrorMessageHandler(Func<EventArgs, Task> errorMessageHandler)
        {
            _serviceBusProcessor.ProcessErrorAsync += errorMessageHandler;
        }

        public void AddMessageHandler(Func<EventArgs, Task> messageHandler)
        {
            _serviceBusProcessor.ProcessMessageAsync += messageHandler;
        }

        public async Task StartAsync() => await _serviceBusProcessor.StartProcessingAsync();
    }
}
