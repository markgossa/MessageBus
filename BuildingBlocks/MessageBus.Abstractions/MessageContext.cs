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
        public string? MessageId { get; internal set; }
        public string? CorrelationId { get; internal set; }
        public Dictionary<string, string> Properties { get; internal set; }
        public int DeliveryCount { get; internal set; }
        private readonly object _messageObject;
        private readonly IMessageBus _messageBus;

        public MessageContext(BinaryData body, object messageObject, IMessageBus messageBus)
        {
            Body = body;
            _messageObject = messageObject;
            _messageBus = messageBus;
            Properties = new Dictionary<string, string>();
        }

        public async Task DeadLetterMessageAsync(string? reason = null)
            => await _messageBus.DeadLetterMessageAsync(_messageObject, reason);

        public async Task PublishAsync(Message<IEvent> eventObject)
        {
            eventObject.CorrelationId ??= CorrelationId;
            await _messageBus.PublishAsync(eventObject);
        }
    }
}
