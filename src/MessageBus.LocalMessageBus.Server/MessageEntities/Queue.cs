using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Concurrent;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public class Queue : IQueue
    {
        private readonly ConcurrentQueue<LocalMessage> _queue = new ();

        public void Send(LocalMessage message) => _queue.Enqueue(message);

        public LocalMessage? Receive()
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
