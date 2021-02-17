using Azure.Identity;
using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MessageBus.microsoft.ServiceBus.Tests.Unit")]

namespace MessageBus.Microsoft.ServiceBus
{
    public class AzureServiceBusClient : IMessageBusClient
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;
        private readonly ServiceBusSender _serviceBusSender;
        private Func<MessageErrorReceivedEventArgs, Task>? _errorMessageHandler;
        private Func<MessageReceivedEventArgs, Task>? _messageHandler;

        public AzureServiceBusClient(string connectionString, string topic, string subscription, 
            ServiceBusProcessorOptions? serviceBusProcessorOptions = null)
        {
            var serviceBusClient = new ServiceBusClient(connectionString);
            _serviceBusProcessor = BuildServiceBusProcessor(serviceBusClient, topic, subscription,
                            serviceBusProcessorOptions);
            _serviceBusSender = serviceBusClient.CreateSender(topic);
            AddMessageHandlers();
        }

        public AzureServiceBusClient(string hostname, string topic, string subscription,
            string tenantId, ServiceBusProcessorOptions? serviceBusProcessorOptions = null)
        {
            var options = new DefaultAzureCredentialOptions
            {
                SharedTokenCacheTenantId = tenantId
            };

            var serviceBusClient = new ServiceBusClient(hostname, new DefaultAzureCredential(options));
            _serviceBusProcessor = BuildServiceBusProcessor(serviceBusClient, topic, subscription,
                            serviceBusProcessorOptions);
            _serviceBusSender = serviceBusClient.CreateSender(topic);
            AddMessageHandlers();
        }

        public void AddMessageHandler(Func<MessageReceivedEventArgs, Task> messageHandler)
            => _messageHandler = messageHandler;

        public void AddErrorMessageHandler(Func<MessageErrorReceivedEventArgs, Task> errorMessageHandler)
            => _errorMessageHandler = errorMessageHandler;

        public async Task StartAsync() => await _serviceBusProcessor.StartProcessingAsync();

        public async Task StopAsync() => await _serviceBusProcessor.StopProcessingAsync(new CancellationToken());

        public async Task DeadLetterMessageAsync(object message, string? reason = null)
            => await ((ProcessMessageEventArgs)message).DeadLetterMessageAsync(((ProcessMessageEventArgs)message).Message, reason);

        public async Task PublishAsync(Message<IEvent> eventMessage) => await SendMessageAsync(eventMessage);

        public async Task SendAsync(Message<ICommand> command) => await SendMessageAsync(command);

        private async Task SendMessageAsync<T>(Message<T> eventMessage) where T : IMessage
        {
            var message = new ServiceBusMessage(BuildMessageBody(eventMessage));
            AddMessageProperties(eventMessage, message);

            await _serviceBusSender.SendMessageAsync(message);
        }

        private ServiceBusProcessor BuildServiceBusProcessor(ServiceBusClient serviceBusClient, string topic,
            string subscription, ServiceBusProcessorOptions? serviceBusProcessorOptions)
        => serviceBusProcessorOptions is null
            ? serviceBusClient.CreateProcessor(topic, subscription)
            : serviceBusClient.CreateProcessor(topic, subscription, serviceBusProcessorOptions);

        private void AddMessageHandlers()
        {
            _serviceBusProcessor.ProcessMessageAsync += CallMessageHandlerAsync;
            _serviceBusProcessor.ProcessErrorAsync += CallErrorMessageHandlerAsync;
        }

        private async Task CallMessageHandlerAsync(ProcessMessageEventArgs args)
        {
            var messageReceivedEventArgs = new MessageReceivedEventArgs(args.Message.Body,
                args, MapToMessageProperties(args.Message.ApplicationProperties))
            {
                MessageId = args.Message.MessageId,
                CorrelationId = args.Message.CorrelationId,
                DeliveryCount = args.Message.DeliveryCount
            };

            ThrowIfMessageHandlerNotFound(messageReceivedEventArgs.MessageId);

            await _messageHandler!(messageReceivedEventArgs);
        }

        internal async Task CallErrorMessageHandlerAsync(ProcessErrorEventArgs args)
        {
            ThrowIfErrorMessageHandlerNotFound();

            await _errorMessageHandler!(new MessageErrorReceivedEventArgs(args.Exception));
        }

        private void ThrowIfMessageHandlerNotFound(string messageId)
        {
            if (_messageHandler is null)
            {
                throw new MessageHandlerNotFoundException($"No message handler found on " +
                    $"{nameof(AzureServiceBusClient)} for MessageId: {messageId}");
            }
        }
        
        private void ThrowIfErrorMessageHandlerNotFound()
        {
            if (_errorMessageHandler is null)
            {
                throw new MessageHandlerNotFoundException($"No error message handler found on " +
                    $"{nameof(AzureServiceBusClient)}");
            }
        }

        private static Dictionary<string, string> MapToMessageProperties(IReadOnlyDictionary<string, object>
            applicationProperties)
        {
            var messageProperties = new Dictionary<string, string>();
            foreach (var item in applicationProperties)
            {
                var value = item.Value.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    messageProperties.Add(item.Key, value);
                }
            }

            return messageProperties;
        }

        private static string? BuildMessageBody<T>(Message<T> message) where T : IMessage
            => message.Body != null
                ? JsonSerializer.Serialize<object>(message.Body)
                : message.BodyAsString;

        private static void AddMessageProperties<T>(Message<T> message, ServiceBusMessage serviceBusMessage)
            where T : IMessage
        {
            foreach (var property in message.MessageProperties)
            {
                serviceBusMessage.ApplicationProperties.Add(property.Key, property.Value);
            }
        }
    }
}
