using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusAdmin
    {
        Task ConfigureAsync(List<Type> messageHandlers);
    }
}