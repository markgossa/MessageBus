﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MessageBus.Abstractions.Tests.Unit")]

namespace MessageBus.Abstractions
{
    public class MessageBusReceiver : IMessageBusReceiver
    {
        private readonly IMessageBusHandlerResolver _messageBusHandlerResolver;
        private readonly IMessageBusAdminClient _messageBusAdminClient;
        private readonly IMessageBusClient _messageBusClient;
        private readonly string _messageTypeProperty;

        public MessageBusReceiver(IMessageBusHandlerResolver messageBusHandlerResolver,
            IMessageBusAdminClient messageBusAdmin, IMessageBusClient messageBusClient, 
            MessageBusReceiverSettings messageBusSettings = null)
        {
            _messageBusHandlerResolver = messageBusHandlerResolver;
            _messageBusAdminClient = messageBusAdmin;
            _messageBusClient = messageBusClient;
            _messageTypeProperty = GetMessageTypeProperty(messageBusSettings);
        }

        public async Task StartAsync() => await _messageBusClient.StartAsync();

        public async Task ConfigureAsync()
        {
            await _messageBusAdminClient.ConfigureAsync(_messageBusHandlerResolver.GetMessageHandlers());
            _messageBusClient.AddMessageHandler(OnMessageReceived);
            _messageBusClient.AddErrorMessageHandler(OnErrorMessageReceived);
        }

        internal async Task OnMessageReceived(MessageReceivedEventArgs args)
        {
            const string handlerHandleMethodName = "HandleAsync";

            var handler = _messageBusHandlerResolver.Resolve(args.MessageProperties[_messageTypeProperty]);

            var messageTypeType = GetMessageTypeFromHandler(handler);
            var messageContextType = typeof(MessageContext<>).MakeGenericType(messageTypeType);
            var messageContext = Activator.CreateInstance(messageContextType, new object[] { args.Message });

            var handlerTask = handler.GetType().GetMethod(handlerHandleMethodName).Invoke(handler, new object[] { messageContext });
            await (handlerTask as Task);
        }

        internal async Task OnErrorMessageReceived(MessageErrorReceivedEventArgs args)
            => await Task.Run(() => throw new MessageReceivedException(args.Exception));

        private static string GetMessageTypeProperty(MessageBusReceiverSettings messageBusSettings)
            => string.IsNullOrWhiteSpace(messageBusSettings?.MessageTypeProperty)
                ? "MessageType"
                : messageBusSettings.MessageTypeProperty;

        private static Type GetMessageTypeFromHandler(object handler) 
            => handler.GetType().GetInterfaces()
                .First(i => i.Name.Contains(typeof(IHandleMessages<>).Name))
                .GenericTypeArguments.First();
    }
}
