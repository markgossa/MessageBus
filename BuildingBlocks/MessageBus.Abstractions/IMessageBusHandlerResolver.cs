using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public interface IMessageBusHandlerResolver
    {
        object Resolve(Type messageType);
        IEnumerable<Type> GetMessageHandlers();
    }
}