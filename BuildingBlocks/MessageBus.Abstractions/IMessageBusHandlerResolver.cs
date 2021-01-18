using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public interface IMessageBusHandlerResolver
    {
        object Resolve(string messageType);
        IEnumerable<Type> GetMessageHandlers();
    }
}