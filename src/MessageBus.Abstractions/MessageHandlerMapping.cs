using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class MessageHandlerMapping
    {
        public Type MessageType { get; }
        public Type MessageHandlerType { get; }
        public Dictionary<string, string> CustomSubscriptionFilterProperties { get; }

        public MessageHandlerMapping(Type messageType, Type messageHandlerType, 
            Dictionary<string, string>? customSubscriptionFilterProperties = null)
        {
            MessageType = messageType;
            MessageHandlerType = messageHandlerType;
            CustomSubscriptionFilterProperties = customSubscriptionFilterProperties ?? new Dictionary<string, string>();
        }
    }
}
