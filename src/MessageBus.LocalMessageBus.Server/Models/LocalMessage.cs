using System.Collections.Generic;

namespace MessageBus.LocalMessageBus.Server.Models
{
    public class LocalMessage
    {
        public string? Body { get; set; }
        public string? Label { get; set; }
        public Dictionary<string, string>? MessageProperties { get; set; }

        public LocalMessage()
        {

        }
        
        public LocalMessage(string body)
        {
            Body = body;
            MessageProperties = new();
        }
    }
}
