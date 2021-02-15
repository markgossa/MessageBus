using MessageBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    internal class PublishingService : IPublishingService
    {
        private readonly ISomeDependency _someDependency;
        private readonly IMessageBus _messageBus;

        public PublishingService(IMessageBus messageBus, ISomeDependency someDependency)
        {
            _someDependency = someDependency;
            _messageBus = messageBus;
        }

        public async Task PublishAsync(IEvent message)
        {
            _someDependency.Ids.Add(Guid.NewGuid().ToString());
            await _messageBus.PublishAsync(new Message<IEvent>(message));
        }
    }
}
