using System;

namespace MessageBus.Abstractions
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public BinaryData Message { get; }

        public MessageReceivedEventArgs(BinaryData message)
        {
            Message = message;
        }
    }
}
