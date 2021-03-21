using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageHandlerResolver : IMessageHandlerResolver
    {
        private readonly IServiceCollection _services;
        private ServiceProvider? _serviceProvider;
        private readonly Dictionary<string, MessageHandlerMapping> _messageSubscriptions = new Dictionary<string, MessageHandlerMapping>();

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
                return (_serviceProvider?.GetRequiredService(handlerServiceType))!;
            }
            catch (Exception ex)
            {
                throw new MessageHandlerNotFoundException($"Message handler for message type {messageType} was not found", ex);
            }
        }

        public IEnumerable<MessageHandlerMapping> GetMessageHandlerMappings() => _messageSubscriptions.Values;

        public void SubcribeToMessage<TMessage, TMessageHandler>(SubscriptionFilter subscriptionFilter)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>
        {
            ThrowIfNullSubscriptionFilter(subscriptionFilter);

            _services.AddTransient(typeof(IMessageHandler<>).MakeGenericType(typeof(TMessage)), typeof(TMessageHandler));

            _messageSubscriptions.Add(subscriptionFilter.EffectiveMessageLabel,
                new MessageHandlerMapping(typeof(TMessage), typeof(TMessageHandler), subscriptionFilter
                    ?? throw new ArgumentNullException(nameof(subscriptionFilter))));
        }

        private static void ThrowIfNullSubscriptionFilter(SubscriptionFilter subscriptionFilter)
        {
            if (subscriptionFilter is null)
            {
                throw new ArgumentNullException(nameof(subscriptionFilter));
            }
        }
    }
}
