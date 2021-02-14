﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MessageBus.Abstractions.Tests.Unit")]

namespace MessageBus.Abstractions
{
    public class MessageBus : IMessageBus
    {
        private readonly IMessageHandlerResolver _messageHandlerResolver;
        private readonly IMessageBusAdminClient _messageBusAdminClient;
        private readonly IMessageBusClient _messageBusClient;
        private readonly string _messageTypePropertyName;
        private readonly MessageBusOptions _messageBusOptions;

        public MessageBus(IMessageHandlerResolver messageHandlerResolver,
            IMessageBusAdminClient messageBusAdmin, IMessageBusClient messageBusClient, 
            MessageBusOptions? messageBusOptions = null)
        {
            _messageHandlerResolver = messageHandlerResolver;
            _messageBusAdminClient = messageBusAdmin;
            _messageBusClient = messageBusClient;
            _messageBusOptions = messageBusOptions ?? new MessageBusOptions();
            _messageTypePropertyName = _messageBusOptions.MessageTypePropertyName;
        }

        public async Task StartAsync()
        {
            await _messageBusClient.StartAsync();
            _messageBusClient.AddMessageHandler(OnMessageReceived);
            _messageBusClient.AddErrorMessageHandler(OnErrorMessageReceived);
        }

        public async Task ConfigureAsync()
        {
            _messageHandlerResolver.Initialize();
            await _messageBusAdminClient.ConfigureAsync(_messageHandlerResolver.GetMessageSubscriptions(),
                _messageBusOptions);
        }

        public async Task DeadLetterMessageAsync(object message, string? reason = null) 
            => await _messageBusClient.DeadLetterMessageAsync(message, reason);

        public async Task<bool> CheckHealthAsync() => await _messageBusAdminClient.CheckHealthAsync();

        public Task StopAsync() => _messageBusClient.StopAsync();

        public IMessageBus SubscribeToMessage<TMessage, TMessageHandler>(Dictionary<string, string>? messageProperties = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>
        {
            _messageHandlerResolver.SubcribeToMessage<TMessage, TMessageHandler>(messageProperties);

            return this;
        }

        internal async Task OnMessageReceived(MessageReceivedEventArgs args)
        {
            const string handlerHandleMethodName = "HandleAsync";

            var handler = _messageHandlerResolver.Resolve(args.MessageProperties[_messageTypePropertyName]);
            var result = handler?.GetType()?.GetMethod(handlerHandleMethodName)?.Invoke(handler, new object[] { BuildMessageContext(args, handler) });
            var handlerTask = result as Task;
            ThrowIfNullHandler(handler, handlerTask, args.MessageId);
            
            await handlerTask!;
        }

        private static void ThrowIfNullHandler(object? handler, Task? handlerTask, string messageId)
        {
            if (handlerTask is null || handler is null)
            {
                throw new MessageHandlerNotFoundException($"Message handler not found or could not be awaited for MessageId: {messageId}");
            }
        }

        private object BuildMessageContext(MessageReceivedEventArgs args, object handler)
        {
            dynamic? messageContext = Activator.CreateInstance(BuildMessageContextType(handler), 
                new object[] { args.Message, args.MessageObject, this });
            messageContext.MessageId = args.MessageId;
            messageContext.CorrelationId = args.CorrelationId;
            messageContext.Properties = args.MessageProperties;
            messageContext.DeliveryCount = args.DeliveryCount;

            return messageContext;
        }

        private static Type BuildMessageContextType(object handler)
            => typeof(MessageContext<>).MakeGenericType((Type)GetMessageTypeFromHandler(handler));

        public async Task PublishAsync(Message<IEvent> eventObject)
        {
            AddMessageProperties(eventObject);

            await _messageBusClient.PublishAsync(eventObject);
        }

        public async Task SendAsync(Message<ICommand> command)
        {
            AddMessageProperties(command);

            await _messageBusClient.SendAsync(command);
        }

        internal async Task OnErrorMessageReceived(MessageErrorReceivedEventArgs args)
            => await Task.Run(() => throw new MessageReceivedException(args.Exception));

        private static Type GetMessageTypeFromHandler(object handler)
            => handler.GetType().GetInterfaces()
                .First(i => i.Name.Contains(typeof(IMessageHandler<>).Name))
                .GenericTypeArguments.First();

        private void AddMessageProperties<T>(Message<T> message) where T : IMessage
        {
            if (!message.OverrideDefaultMessageProperties)
            {
                AddMessageTypeProperty(message);
                AddMessageVersionProperty(message);
            }
        }

        private void AddMessageTypeProperty<T>(Message<T> message) where T : IMessage
        {
            if (message.Body != null)
            {
                message.MessageProperties.Add(_messageBusOptions.MessageTypePropertyName, message.Body.GetType().Name);
            }
        }

        private void AddMessageVersionProperty<T>(Message<T> message) where T : IMessage
        {
            var messageVersion = GetMessageVersion(message);
            if (messageVersion != null)
            {
                message.MessageProperties.Add(_messageBusOptions.MessageVersionPropertyName, messageVersion);
            }
        }

        private static string? GetMessageVersion<T>(Message<T> message) where T : IMessage
            => message.Body?.GetType().CustomAttributes.FirstOrDefault(b =>
                b.AttributeType == typeof(MessageVersionAttribute))?.ConstructorArguments.FirstOrDefault().Value?.ToString();
    }
}
