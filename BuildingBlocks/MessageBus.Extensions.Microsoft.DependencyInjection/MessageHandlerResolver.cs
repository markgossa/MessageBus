using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageHandlerResolver : IMessageHandlerResolver
    {
        private readonly IServiceCollection _services;
        private ServiceProvider _serviceProvider;
        private readonly Dictionary<string, MessageSubscription> _messageSubscriptions = new Dictionary<string, MessageSubscription>();

        public MessageHandlerResolver(IServiceCollection services)
        {
            _services = services;
        }
        
        public void Initialize() => _serviceProvider = _services.BuildServiceProvider();

        public object Resolve(string messageType)
        {
            try
            {
                var messageTypeType = _messageSubscriptions[messageType].MessageType;
                var handlerServiceType = typeof(IMessageHandler<>).MakeGenericType(messageTypeType);
                return _serviceProvider.GetRequiredService(handlerServiceType);
            }
            catch (Exception ex)
            {
                throw new MessageHandlerNotFoundException($"Message handler for message type {messageType} was not found", ex);
            }
        }

        public IEnumerable<MessageSubscription> GetMessageSubscriptions() => _messageSubscriptions.Values;

        public void SubcribeToMessage<TMessage, TMessageHandler>(Dictionary<string, string> messageProperties = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>
        {
            _services.AddScoped(typeof(IMessageHandler<>).MakeGenericType(typeof(TMessage)), typeof(TMessageHandler));
            _messageSubscriptions.Add(typeof(TMessage).Name, new MessageSubscription(typeof(TMessage),typeof(TMessageHandler), 
                messageProperties));
        }
    }
}
