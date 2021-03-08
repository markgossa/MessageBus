using System;
using System.Collections.Generic;

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

        private Message(string? correlationId = null, string? messageId = null,
            Dictionary<string, string>? messageProperties = null)
        {
            MessageProperties = messageProperties ?? new Dictionary<string, string>();
            CorrelationId = correlationId;
            MessageId = messageId ?? Guid.NewGuid().ToString();
        }
    }
}
