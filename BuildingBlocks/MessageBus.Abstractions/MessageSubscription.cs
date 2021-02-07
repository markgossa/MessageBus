using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class MessageSubscription
    {
        public Type MessageType { get; }
        public Type MessageHandlerType { get; }
        public Dictionary<string, string> CustomSubscriptionFilterProperties { get; }

        public MessageSubscription(Type messageType, Type messageHandlerType, 
            Dictionary<string, string>? customSubscriptionFilterProperties = null)
        {
            MessageType = messageType;
            MessageHandlerType = messageHandlerType;
            CustomSubscriptionFilterProperties = customSubscriptionFilterProperties ?? new Dictionary<string, string>();
        }
    }
}
