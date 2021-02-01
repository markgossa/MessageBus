using System;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusClient
    {
        Task StartAsync();
        void AddMessageHandler(Func<MessageReceivedEventArgs, Task> messageHandler);
        void AddErrorMessageHandler(Func<MessageErrorReceivedEventArgs, Task> errorMessageHandler);
        Task DeadLetterMessageAsync(object message, string reason = null);
    }
}
