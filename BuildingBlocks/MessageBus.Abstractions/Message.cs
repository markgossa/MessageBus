using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public abstract class Message
    {
        public string? MessageAsString { get; }
        public string CorrelationId { get; set; }
        public string MessageId { get; set; }
        public Dictionary<string, string> MessageProperties { get; set; }

        protected Message(string? correlationId = null, string? messageId = null,
            Dictionary<string, string>? messageProperties = null)
        {
            MessageProperties = messageProperties ?? new Dictionary<string, string>();
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            MessageId = messageId ?? Guid.NewGuid().ToString();
        }

        protected Message(string eventString) : this()
        {
            MessageAsString = eventString;
        }
    }
}
