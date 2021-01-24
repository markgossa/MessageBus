using System;

namespace MessageBus.Abstractions
{
    public class MessageErrorContext
    {
        public Exception Exception { get; }

        public MessageErrorContext(Exception exception)
        {
            Exception = exception;
        }
    }
}
