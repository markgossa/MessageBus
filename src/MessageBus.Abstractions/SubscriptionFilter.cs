using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class SubscriptionFilter
    {
        public Dictionary<string, string> MessageProperties { get; set; } = new Dictionary<string, string>();
        public string? Label
        {
            get
            {
                if (!IsValidBuildParameters())
                {
                    throw new InvalidOperationException($"{nameof(SubscriptionFilter)} must be built before use");
                }

                if (MessageTypePropertyFound())
                {
                    return null;
                }

                return string.IsNullOrWhiteSpace(_label)
                   ? _messageTypeType
                   : _label;
            }

            set => _label = value;
        }

        private bool IsValidBuildParameters() 
            => !string.IsNullOrWhiteSpace(_messageTypePropertyName) && _messageTypeType != null;

        private bool MessageTypePropertyFound() 
            => MessageProperties.TryGetValue(_messageTypePropertyName!, out var messageType)
                && !string.IsNullOrWhiteSpace(messageType);

        private string? _label;
        private string? _messageTypePropertyName;
        private string? _messageTypeType;

        public void Build(string messageTypePropertyName, Type message)
        {
            (_messageTypePropertyName, _messageTypeType) = (messageTypePropertyName, message?.Name);

            if (!IsValidBuildParameters())
            {
                throw new ArgumentNullException($"{nameof(messageTypePropertyName)}, {nameof(message)}");
            }
        }
    }
}
