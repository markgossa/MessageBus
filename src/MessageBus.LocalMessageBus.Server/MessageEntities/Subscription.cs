using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public class Subscription : ISubscription
    {
        public string Name { get; private set; }
        public string? Label { get; set; }
        public Dictionary<string, string> MessageProperties { get; set; }

        private readonly IQueue _queue;

        public Subscription(IQueue queue, string name)
        {
            _queue = queue;
            MessageProperties = new();
            Name = name;
        }

        public void Send(LocalMessage message)
        {
            if (IsMatchingLabel(message) && IsMatchingMessageProperties(message))
            {
                _queue.Send(message);
            }
        }

        public LocalMessage? Receive() => _queue.Receive();
        
        private bool IsMatchingLabel(LocalMessage message) => message.Label == Label;

        private bool IsMatchingMessageProperties(LocalMessage message)
        {
            var filterProperties = MessageProperties.AsEnumerable();
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
