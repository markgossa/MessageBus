using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Processors
{
    internal class TestMessageProcessor : IMessagePreProcessor, IMessagePostProcessor
    {
        private readonly IMessageTracker _messageTracker;
        private readonly ISendingService _sendingService;

        public TestMessageProcessor(IMessageTracker messageTracker, ISendingService sendingService)
        {
            _messageTracker = messageTracker;
            _sendingService = sendingService;
        }

        public async Task ProcessAsync<T>(IMessageContext<T> context) where T : IMessage
        {
            _messageTracker.Ids.Add(context.MessageId);

            await _sendingService.SendAsync(new SetAutopilot { AutopilotId = context.MessageId });
        }
    }
}
