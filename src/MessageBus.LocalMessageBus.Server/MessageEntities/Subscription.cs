using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public class Subscription : ISubscription
    {
        public string Name { get; set; }
        public string? Label { get; set; }
        public Dictionary<string, string> MessageProperties
        {
            get => _messageProperties;
            set => _messageProperties = value ?? new();
        }

        private readonly IQueue? _queue;
        private Dictionary<string, string> _messageProperties = new();

        public Subscription()
        {
            MessageProperties = new();
        }

        public Subscription(string name, IQueue? queue = null)
        {
            _queue = queue;
            MessageProperties = new Dictionary<string, string>();
            Name = name;
        }

        public void Send(LocalMessage message)
        {
            if (IsMatchingLabel(message) && IsMatchingMessageProperties(message))
            {
                _queue?.Send(message);
            }
        }

        public LocalMessage? Receive() => _queue?.Receive();
        
        private bool IsMatchingLabel(LocalMessage message) => message.Label == Label;

        private bool IsMatchingMessageProperties(LocalMessage message)
        {
            if (MessageProperties.Count == 0)
            { 
                return true; 
            }    

            var filterProperties = MessageProperties?.AsEnumerable();
            foreach (var filterProperty in filterProperties)
            {
                if (!message.MessageProperties.TryGetValue(filterProperty.Key, out var value)
                    || value != filterProperty.Value)
                {
                    return false;
                }
            }

            return filterProperties.Count() == message.MessageProperties.Count;
        }
    }
}
