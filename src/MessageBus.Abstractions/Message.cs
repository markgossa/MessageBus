using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Abstractions
{
    public class Message<T> where T : IMessage
    {
        public T Body { get; }
        public string? BodyAsString { get; }
        public string? CorrelationId { get; set; }
        public string MessageId { get; set; }
        public Dictionary<string, string> MessageProperties { get; set; }
        public bool OverrideDefaultMessageProperties { get; set; }
        public DateTimeOffset ScheduledEnqueueTime { get; set; }
        public string? Label
        {
            get => !IsMessageTypeSpecified()
                  ? GetLabel()
                  : null;

            set => _label = value;
        }

        private MessageBusOptions? _messageBusOptions;
        private string? _label;

        public Message(T body, string? correlationId = null, string? messageId = null,
            Dictionary<string, string>? messageProperties = null) : this(correlationId,
                messageId, messageProperties)
        {
            Body = body;
        }

        public Message(string body, string? label, string? correlationId = null, string? messageId = null,
            Dictionary<string, string>? messageProperties = null) : this(correlationId,
                messageId, messageProperties)
        {
            BodyAsString = body;
            _label = label;
        }

        internal void Build(MessageBusOptions messageBusOptions)
        {
            _messageBusOptions = messageBusOptions;
            AddMessageVersionProperty();
            ValidateLabelOrMessageTypePropertyPresent();
        }

        #nullable disable
        private Message(string correlationId, string messageId,
            Dictionary<string, string> messageProperties)
        {
            MessageProperties = messageProperties ?? new Dictionary<string, string>();
            CorrelationId = correlationId;
            MessageId = messageId ?? Guid.NewGuid().ToString();
        }
        # nullable enable

        private bool IsMessageTypeSpecified()
            => _messageBusOptions != null
                && MessageProperties.TryGetValue(_messageBusOptions!.MessageTypePropertyName, out var messageType)
                && !string.IsNullOrWhiteSpace(messageType);

        private string? GetLabel()
            => !string.IsNullOrWhiteSpace(_label)
                ? _label
                : Body?.GetType().Name;

        private void AddMessageVersionProperty()
        {
            var messageVersion = Body?.GetType().CustomAttributes.FirstOrDefault(b =>
                b.AttributeType == typeof(MessageVersionAttribute))?.ConstructorArguments
                .FirstOrDefault().Value?.ToString();

            if (messageVersion != null && _messageBusOptions != null && !OverrideDefaultMessageProperties)
            {
                MessageProperties.Add(_messageBusOptions.MessageVersionPropertyName, messageVersion);
            }
        }

        private void ValidateLabelOrMessageTypePropertyPresent()
        {
            if (Label is null && !IsMessageTypeSpecified())
            {
                throw new ArgumentNullException(nameof(Label));
            }
        }
    }
}
