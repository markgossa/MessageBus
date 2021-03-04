using System.Collections.Generic;

#nullable disable
namespace MessageBus.LocalMessageBus.Server.Models
{
    public class SubscriptionRequest
    {
        public string Name { get; set; }
        public Dictionary<string, string> MessageProperties { get; set; }
        public string Label { get; set; }
    }
}
