using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageContext<TMessage> where TMessage : IMessage
    {
        BinaryData Body { get; }
        TMessage Message { get; }
        string? MessageId { get; }
        string? CorrelationId { get; }
        Dictionary<string, string> Properties { get; }
        int DeliveryCount { get; }

        Task DeadLetterMessageAsync(string? reason = null);
        Task PublishAsync(Message<IEvent> eventMessage);
        Task SendAsync(Message<ICommand> command);
        Task SendMessageCopyAsync(int delayInSeconds = 0);
        Task SendMessageCopyAsync(DateTimeOffset enqueueTime);
    }
}
 