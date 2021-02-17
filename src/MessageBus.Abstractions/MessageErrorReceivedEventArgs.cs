using System;

namespace MessageBus.Abstractions
{
    public class MessageErrorReceivedEventArgs
    {
        public Exception Exception { get; }

        public MessageErrorReceivedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
