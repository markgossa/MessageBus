using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Abstractions
{
    public class SubscriptionFilter
    {
        public Dictionary<string, string> MessageProperties 
        { 
            get => _messageProperties; 
            set => _messageProperties = value ?? new Dictionary<string, string>(); 
        }

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

        private string? _label;
        private MessageBusOptions? _messageBusOptions;
        private string? _messageTypeType;
        private Dictionary<string, string> _messageProperties = new Dictionary<string, string>();

        private bool IsValidBuildParameters() 
            => _messageBusOptions != null && _messageTypeType != null;

        private bool MessageTypePropertyFound() 
            => MessageProperties.TryGetValue(_messageBusOptions!.MessageTypePropertyName, out var messageType)
                && !string.IsNullOrWhiteSpace(messageType);


        public void Build(MessageBusOptions messageBusOptions, Type message)
        {
            (_messageBusOptions, _messageTypeType) = (messageBusOptions, message?.Name);

            if (!IsValidBuildParameters())
            {
                throw new ArgumentNullException($"{nameof(messageBusOptions)}, {nameof(message)}");
            }

            AddMessageVersionProperty(message!);
        }

        private void AddMessageVersionProperty(Type message)
        {
            if (!MessageProperties.Any() && message != null)
            {
                var messageVersion = GetMessageVersion(message);

                if (messageVersion != null)
                {
                    MessageProperties.Add(_messageBusOptions!.MessageVersionPropertyName, messageVersion);
                }
            }
        }

        private static string? GetMessageVersion(Type message) 
            => message.CustomAttributes.FirstOrDefault(b => b.AttributeType == typeof(MessageVersionAttribute))?
                .ConstructorArguments.FirstOrDefault().Value?.ToString();
    }
}
