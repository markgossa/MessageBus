using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class MessageContext
    {
        public BinaryData Message { get; }
        public Dictionary<string, string> MessageProperties = new Dictionary<string, string>();

        public MessageContext(BinaryData message, Dictionary<string, string> messageProperties)
        {
            Message = message;
            MessageProperties = messageProperties;
        }
    }
}
