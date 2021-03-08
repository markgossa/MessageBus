using MessageBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    internal class SendingServiceWithDelay : ISendingService
    {
        private readonly IMessageBus _messageBus;
        private readonly IMessageTracker _messageTracker;

        public SendingServiceWithDelay(IMessageBus messageBus, IMessageTracker messageTracker)
        {
            _messageBus = messageBus;
            _messageTracker = messageTracker;
        }

        public async Task SendAsync(ICommand message)
        {
            _messageTracker.Ids.Add(Guid.NewGuid().ToString());
            await _messageBus.SendAsync(new Message<ICommand>(message) { ScheduledEnqueueTime = DateTimeOffset.Now.AddSeconds(10) });
        }
    }
}
