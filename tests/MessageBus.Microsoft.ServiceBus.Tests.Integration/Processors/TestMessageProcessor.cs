using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Processors
{
    internal class TestMessageProcessor : IMessagePreProcessor, IMessagePostProcessor
    {
        private readonly ISomeDependency _someDependency;
        private readonly ISendingService _sendingService;

        public TestMessageProcessor(ISomeDependency someDependency, ISendingService sendingService)
        {
            _someDependency = someDependency;
            _sendingService = sendingService;
        }

        public async Task ProcessAsync<T>(IMessageContext<T> context) where T : IMessage
        {
            _someDependency.Ids.Add(context.MessageId);

            await _sendingService.SendAsync(new SetAutopilot { AutopilotId = context.MessageId });
        }
    }
}
