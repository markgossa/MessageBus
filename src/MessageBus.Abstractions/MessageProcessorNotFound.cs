using System;
using System.Collections.Generic;
using System.Text;

namespace MessageBus.Abstractions
{
    public class MessageProcessorNotFound : Exception
    {
        public MessageProcessorNotFound(string message) : base(message)
        {
        }
    }
}
