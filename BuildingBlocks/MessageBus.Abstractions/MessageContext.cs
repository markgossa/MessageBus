using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MessageBus.Abstractions
{
    public class MessageContext<TMessage> : IMessageContext<TMessage> where TMessage : IMessage
    {
        public BinaryData Body { get; private set; }
        public TMessage Message => JsonSerializer.Deserialize<TMessage>(Body.ToString());
        public string MessageId { get; internal set; }
        public string CorrelationId { get; internal set; }
        public Dictionary<string, string> Properties { get; internal set; }
        public int DeliveryCount { get; internal set; }

        public MessageContext(BinaryData body)
        {
            Body = body;
        }
    }
}
