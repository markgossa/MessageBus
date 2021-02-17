using System;
using System.Collections.Concurrent;

namespace MessageBus.HostedService.Example.Services
{
    public class SomeDependency : IDependency
    {
        private readonly ConcurrentDictionary<Guid, Guid> _receivedMessages = new ConcurrentDictionary<Guid, Guid>();

        public void SaveMessageId(Guid messageId)
        {
            Console.WriteLine(messageId);
            _receivedMessages.TryAdd(messageId, messageId);
        }
    }
}
