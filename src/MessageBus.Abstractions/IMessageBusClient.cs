﻿using System;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusClient
    {
        Task StartAsync();
        Task StopAsync();
        void AddMessageHandler(Func<MessageReceivedEventArgs, Task> messageHandler);
        void AddErrorMessageHandler(Func<MessageErrorReceivedEventArgs, Task> errorMessageHandler);
        Task DeadLetterMessageAsync(object message, string? reason = null);
        Task PublishAsync(Message<IEvent> eventObject);
        Task SendAsync(Message<ICommand> command);
    }
}