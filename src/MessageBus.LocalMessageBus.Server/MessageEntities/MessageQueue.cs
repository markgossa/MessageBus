using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Generic;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public class MessageQueue
    {
        private readonly Queue<LocalMessage> _queue = new Queue<LocalMessage>();

        public void Enqueue(LocalMessage message) => _queue.Enqueue(message);

        public LocalMessage Dequeue() => _queue.Dequeue();
    }
}
