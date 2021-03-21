using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    internal class SendingService : ISendingService
    {
        private readonly IMessageBus _messageBus;
        private readonly IMessageTracker _messageTracker;

        public SendingService(IMessageBus messageBus, IMessageTracker messageTracker)
        {
            _messageBus = messageBus;
            _messageTracker = messageTracker;
        }

        public async Task SendAsync(ICommand message)
        {
            _messageTracker.Ids.Add(Guid.NewGuid().ToString());
            await _messageBus.SendAsync(new Message<ICommand>(message));
        }
    }
}
