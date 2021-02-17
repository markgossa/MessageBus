using System;

namespace MessageBus.Abstractions
{
    public class MessageHandlerNotFoundException : Exception
    {
        public MessageHandlerNotFoundException(string message) : base (message) {}
        public MessageHandlerNotFoundException(string message, Exception innerException) : base (message, innerException) {}
    }
}
