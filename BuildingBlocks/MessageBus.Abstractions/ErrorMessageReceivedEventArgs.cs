using System;

namespace MessageBus.Abstractions
{
    public class ErrorMessageReceivedEventArgs
    {
        public Exception Exception { get; }

        public ErrorMessageReceivedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
