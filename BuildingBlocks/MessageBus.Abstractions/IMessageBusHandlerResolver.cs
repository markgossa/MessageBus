using System;

namespace MessageBus.Abstractions
{
    public interface IMessageBusHandlerResolver
    {
        object Resolve(Type messageType);
    }
}