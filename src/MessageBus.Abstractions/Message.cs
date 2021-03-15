using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Abstractions
{
    public class Message<T> where T : IMessage
    {
        public T Body { get; }
        public string? BodyAsString { get; }
        public string? CorrelationId { get; set; }
        public string MessageId { get; set; }
        public Dictionary<string, string> MessageProperties { get; set; }
        public bool OverrideDefaultMessageProperties { get; set; }
        public DateTimeOffset ScheduledEnqueueTime { get; set; }
        public string Label => Body.GetType().Name;

        private MessageBusOptions? _messageBusOptions;

        public Message(T body, string? correlationId = null, string? messageId = null,
            Dictionary<string, string>? messageProperties = null) : this(correlationId, 
                messageId, messageProperties)
        {
            Body = body;
        }

        public Message(string body, string? correlationId = null, string? messageId = null,
            Dictionary<string, string>? messageProperties = null) : this(correlationId,
                messageId, messageProperties)
        {
            BodyAsString = body;
        }

        internal void Build(MessageBusOptions messageBusOptions)
        {
            _messageBusOptions = messageBusOptions;
            AddMessageVersionProperty();
        }

        private Message(string? correlationId = null, string? messageId = null,
            Dictionary<string, string>? messageProperties = null)
        {
            MessageProperties = messageProperties ?? new Dictionary<string, string>();
            CorrelationId = correlationId;
            MessageId = messageId ?? Guid.NewGuid().ToString();
        }

        private void AddMessageVersionProperty()
        {
            var messageVersion = Body.GetType().CustomAttributes.FirstOrDefault(b => 
                b.AttributeType == typeof(MessageVersionAttribute))?.ConstructorArguments
                .FirstOrDefault().Value?.ToString();

            if (messageVersion != null && _messageBusOptions != null && !OverrideDefaultMessageProperties)
            {
                MessageProperties.Add(_messageBusOptions.MessageVersionPropertyName, messageVersion);
            }
        }
    }
}
