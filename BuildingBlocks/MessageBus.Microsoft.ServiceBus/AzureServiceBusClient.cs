using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Utilities;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus
{
    public class AzureServiceBusClient : IMessageBusClient
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;
        private Func<EventArgs, Task> _errorMessageHandler;
        private Func<MessageReceivedEventArgs, Task> _messageHandler;

        public AzureServiceBusClient(string connectionString, string topic, string subscription)
        {
            _serviceBusProcessor = new ServiceBusClient(connectionString).CreateProcessor(topic, subscription);
            AddMessageHandlers();
        }

        public AzureServiceBusClient(string hostname, string topic, string subscription,
            string tenantId = null)
        {
            _serviceBusProcessor = new ServiceBusClient(hostname, new ServiceBusTokenProvider(tenantId))
                .CreateProcessor(topic, subscription);
            AddMessageHandlers();
        }

        public void AddMessageHandler(Func<MessageReceivedEventArgs, Task> messageHandler)
            => _messageHandler = messageHandler;

        public void AddErrorMessageHandler(Func<EventArgs, Task> errorMessageHandler)
            => _errorMessageHandler = errorMessageHandler;

        public async Task StartAsync() => await _serviceBusProcessor.StartProcessingAsync();

        private void AddMessageHandlers()
        {
            _serviceBusProcessor.ProcessMessageAsync += CallMessageHandlerAsync;
            _serviceBusProcessor.ProcessErrorAsync += ThrowException;
        }

        private async Task CallMessageHandlerAsync(ProcessMessageEventArgs args)
        {
            var messageReceivedEventArgs = new MessageReceivedEventArgs(args.Message.Body.ToString());
            await _messageHandler(messageReceivedEventArgs);
        }

        private async Task ThrowException(ProcessErrorEventArgs args) => Console.WriteLine("Some error!");
    }
}
