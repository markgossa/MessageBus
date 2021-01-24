using System;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusClient
    {
        Task StartAsync();
        void AddMessageHandler(Func<MessageContext, Task> messageHandler);
        void AddErrorMessageHandler(Func<MessageErrorContext, Task> errorMessageHandler);
    }
}
