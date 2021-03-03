using System.Collections.Generic;

namespace MessageBus.LocalMessageBus.Server.Models
{
    public class LocalMessage
    {
        public string Body { get; }
        public string? Label { get; init; }
        public Dictionary<string, string> MessageProperties { get; init; }

        public LocalMessage(string body)
        {
            Body = body;
            MessageProperties = new();
        }
    }
}
