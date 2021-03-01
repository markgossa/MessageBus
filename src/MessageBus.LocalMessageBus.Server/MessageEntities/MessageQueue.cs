using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Concurrent;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public class MessageQueue
    {
        private readonly ConcurrentQueue<LocalMessage> _queue = new ();

        public void Enqueue(LocalMessage message) => _queue.Enqueue(message);

        public LocalMessage? Dequeue()
        {
            var retryCount = 3;
            var successfulDequeue = false;
            LocalMessage? message = null;
            for (var i = 0; i < retryCount && !successfulDequeue; i++)
            {
                successfulDequeue = _queue.TryDequeue(out message);
            }

            return message;
        }
    }
}
