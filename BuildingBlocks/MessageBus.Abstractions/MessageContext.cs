using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

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
        private readonly object _messageObject;
        private readonly IMessageBusReceiver _messageBusReceiver;

        public MessageContext(BinaryData body, object messageObject, IMessageBusReceiver messageBusReceiver)
        {
            Body = body;
            _messageObject = messageObject;
            _messageBusReceiver = messageBusReceiver;
        }

        internal async Task DeadLetterAsync() => await _messageBusReceiver.DeadLetterAsync(_messageObject);
    }
}
