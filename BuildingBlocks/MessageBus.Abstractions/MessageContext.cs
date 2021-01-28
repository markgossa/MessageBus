using System;
using System.Text.Json;

namespace MessageBus.Abstractions
{
    public class MessageContext<TMessage> where TMessage : IMessage
    {
        public BinaryData Body { get; private set; }
        public TMessage Message => JsonSerializer.Deserialize<TMessage>(Body.ToString());

        public string MessageId { get; internal set; }

        public MessageContext(BinaryData body, string messageId)
        {
            Body = body;
            MessageId = messageId;
        }
    }
}
