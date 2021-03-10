using System;
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
        IMessageBus SubscribeToMessage<TMessage, TMessageHandler>(SubscriptionFilter? subscriptionFilter = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>;
        Task PublishAsync(Message<IEvent> message);
        Task SendAsync(Message<ICommand> message);
        IMessageBus AddMessagePreProcessor<T>() where T : class, IMessagePreProcessor;
        IMessageBus AddMessagePostProcessor<T>() where T : class, IMessagePostProcessor;
        Task SendMessageCopyAsync(object messageObject, int delayInSeconds = 0);
        Task SendMessageCopyAsync(object messageObject, DateTimeOffset enqueueTime);
    }
}