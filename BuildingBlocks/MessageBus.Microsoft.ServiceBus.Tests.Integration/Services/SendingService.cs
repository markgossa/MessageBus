using MessageBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    internal class SendingService : ISendingService
    {
        private readonly IMessageBus _messageBus;
        private readonly ISomeDependency _someDependency;

        public SendingService(IMessageBus messageBus, ISomeDependency someDependency)
        {
            _messageBus = messageBus;
            _someDependency = someDependency;
        }

        public Task SendAsync(ICommand message)
        {
            _someDependency.Ids.Add(Guid.NewGuid().ToString());
            return _messageBus.SendAsync(new Message<ICommand>(message));
        }
    }
}
