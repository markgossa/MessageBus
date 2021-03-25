using MessageBus.Abstractions.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Abstractions
{
    public class SubscriptionFilter
    {
        public Dictionary<string, string> MessageProperties { get; set; }

        public string? Label { get; set; }

        public string EffectiveMessageLabel
        {
            get
            {
                ThrowIfNotBuilt();
                ThrowIfCannotDetermineEffectiveMessageLabel();
                return Label ?? MessageProperties[_messageBusOptions!.MessageTypePropertyName];
            }
        }

        private MessageBusOptions? _messageBusOptions;
        private string? _messageTypeType;

        public SubscriptionFilter()
        {
            MessageProperties = new Dictionary<string, string>();
        }

        private void ThrowIfNotBuilt()
        {
            var isValid = ValidateBuildParameters();
            if (!isValid)
            {
                throw new InvalidOperationException($"{nameof(SubscriptionFilter)} must be built with valid parameters before use");
            }
        }

        private void ThrowIfCannotDetermineEffectiveMessageLabel()
        {
            if (IsLabelAndMessageTypePropertyEmpty())
            {
                throw new ArgumentNullException($"{nameof(Label)} or {_messageBusOptions!.MessageTypePropertyName}",
                    $"Subscription Filter Label, or {_messageBusOptions!.MessageTypePropertyName} property must be specified");
            }
        }

        private bool IsLabelAndMessageTypePropertyEmpty()
            => string.IsNullOrWhiteSpace(Label)
                && (!MessageProperties.TryGetValue(_messageBusOptions!.MessageTypePropertyName, out var messageType)
                    || string.IsNullOrWhiteSpace(messageType));

        private bool ValidateBuildParameters() => _messageBusOptions != null && _messageTypeType != null;

        private bool MessageTypePropertyFound() 
            => MessageProperties.TryGetValue(_messageBusOptions!.MessageTypePropertyName, out var messageType)
                && !string.IsNullOrWhiteSpace(messageType);


        public void Build(MessageBusOptions messageBusOptions, Type message)
        {
            (_messageBusOptions, _messageTypeType) = (messageBusOptions, message?.Name);

            if (!ValidateBuildParameters())
            {
                throw new ArgumentNullException($"Argument {nameof(messageBusOptions)} or {nameof(message)} are null");
            }

            AddMessageVersionPropertyIfNoCustomMessageProperties(message!);
        }

        private void AddMessageVersionPropertyIfNoCustomMessageProperties(Type message)
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
