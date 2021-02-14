using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBus
    {
        Task ConfigureAsync();
        Task StartAsync();
        Task StopAsync();
        Task DeadLetterMessageAsync(object message, string? reason = null);
        Task<bool> CheckHealthAsync();
        IMessageBus SubscribeToMessage<TMessage, TMessageHandler>(Dictionary<string, string> messageProperties = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>;
        Task PublishAsync(Message<IEvent> message);
    }
}