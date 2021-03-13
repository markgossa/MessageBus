using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class SubscriptionFilter
    {
        public Dictionary<string, string> MessageProperties { get; set; } = new Dictionary<string, string>();
        public string? Label
        {
            get => string.IsNullOrWhiteSpace(_label)
                    ? SubscriptionMessageType?.Name
                    : _label;

            set => _label = value;
        }

        public string? MessageTypePropertyName { get; internal set; }
        public Type? SubscriptionMessageType { get; internal set; }
        private string? _label;
    }
}
