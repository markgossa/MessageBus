using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public BinaryData Message { get; }
        public Dictionary<string, string> MessageProperties = new Dictionary<string, string>();

        public MessageReceivedEventArgs(BinaryData message)
        {
            Message = message;
        }
    }
}
