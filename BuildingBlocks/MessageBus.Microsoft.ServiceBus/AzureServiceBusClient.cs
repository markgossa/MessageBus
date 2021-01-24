﻿using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MessageBus.microsoft.ServiceBus.Tests.Unit")]

namespace MessageBus.Microsoft.ServiceBus
{
    public class AzureServiceBusClient : IMessageBusClient
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;
        private Func<ErrorMessageReceivedEventArgs, Task> _errorMessageHandler;
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

        public void AddErrorMessageHandler(Func<ErrorMessageReceivedEventArgs, Task> errorMessageHandler)
            => _errorMessageHandler = errorMessageHandler;

        public async Task StartAsync() => await _serviceBusProcessor.StartProcessingAsync();

        private void AddMessageHandlers()
        {
            _serviceBusProcessor.ProcessMessageAsync += CallMessageHandlerAsync;
            _serviceBusProcessor.ProcessErrorAsync += CallErrorMessageHandlerAsync;
        }

        private async Task CallMessageHandlerAsync(ProcessMessageEventArgs args)
        {
            var messageReceivedEventArgs = new MessageReceivedEventArgs(args.Message.Body,
                MapToMessageProperties(args.Message.ApplicationProperties));
            await _messageHandler(messageReceivedEventArgs);
        }

        internal async Task CallErrorMessageHandlerAsync(ProcessErrorEventArgs args)
            => await _errorMessageHandler(new ErrorMessageReceivedEventArgs(args.Exception));

        private static Dictionary<string, string> MapToMessageProperties(IReadOnlyDictionary<string, object>
            applicationProperties)
        {
            var messageProperties = new Dictionary<string, string>();
            foreach (var item in applicationProperties)
            {
                messageProperties.Add(item.Key, item.Value.ToString());
            }

            return messageProperties;
        }
    }
}
