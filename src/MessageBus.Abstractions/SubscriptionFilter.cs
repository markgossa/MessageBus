using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class SubscriptionFilter
    {
        public Dictionary<string, string> MessageProperties { get; set; } = new Dictionary<string, string>();
        public string? Label { get; internal set; }
    }
}
