using System;

namespace MessageBus.Abstractions
{
    public class MessageHandlerMapping
    {
        public Type MessageType { get; }
        public Type MessageHandlerType { get; }
        public SubscriptionFilter? SubscriptionFilter { get; }

        public MessageHandlerMapping(Type messageType, Type messageHandlerType, 
            SubscriptionFilter? subscriptionFilter = null)
        {
            MessageType = messageType;
            MessageHandlerType = messageHandlerType;
            SubscriptionFilter = subscriptionFilter ?? throw new ArgumentNullException(nameof(SubscriptionFilter));
        }
    }
}
