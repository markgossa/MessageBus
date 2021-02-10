using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class Event : Message
    {
        public IEvent? Message { get; }

        public Event(IEvent eventObject, string? correlationId = null, string? messageId = null, 
            Dictionary<string, string>? messageProperties = null) : base(correlationId, messageId,
                messageProperties)
        {
            Message = eventObject;
        }

        public Event(string eventString) : base(eventString) { }
    }
}
