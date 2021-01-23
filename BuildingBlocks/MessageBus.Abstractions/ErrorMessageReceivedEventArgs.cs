using System;

namespace MessageBus.Abstractions
{
    public class ErrorMessageReceivedEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ErrorMessageReceivedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
