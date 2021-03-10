using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class SubscriptionFilter
    {
        public Dictionary<string, string> MessageProperties { get; set; }
    }
}
