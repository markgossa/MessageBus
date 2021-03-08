using MessageBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    internal class PublishingServiceWithDelay : IPublishingService
    {
        private readonly IMessageTracker _messageTracker;
        private readonly IMessageBus _messageBus;

        public PublishingServiceWithDelay(IMessageBus messageBus, IMessageTracker messageTracker)
        {
            _messageTracker = messageTracker;
            _messageBus = messageBus;
        }

        public async Task PublishAsync(IEvent message)
        {
            _messageTracker.Ids.Add(Guid.NewGuid().ToString());
            await _messageBus.PublishAsync(new Message<IEvent>(message) { ScheduledEnqueueTime = DateTimeOffset.Now.AddSeconds(10) });
        }
    }
}
