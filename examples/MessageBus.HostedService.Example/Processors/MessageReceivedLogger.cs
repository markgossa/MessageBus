using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MessageBus.HostedService.Example.Processors
{
    public class MessageReceivedLogger : IMessagePreProcessor
    {
        private readonly ILogger<MessageReceivedLogger> _logger;

        public MessageReceivedLogger(ILogger<MessageReceivedLogger> logger)
        {
            _logger = logger;
        }

        public async Task ProcessAsync<T>(IMessageContext<T> context) where T : IMessage
        {
            _logger.LogInformation($"Message received with MessageId: {context.MessageId}");

            await Task.Delay(10);
        }
    }
}
