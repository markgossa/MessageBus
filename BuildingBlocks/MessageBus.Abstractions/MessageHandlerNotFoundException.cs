using System;

namespace MessageBus.Abstractions
{
    public class MessageHandlerNotFoundException : Exception
    {
        public MessageHandlerNotFoundException(string message) : base (message)
        {
        }
    }
}
