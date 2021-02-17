using System;

namespace MessageBus.Abstractions
{
    public class MessageReceivedException : Exception
    {
        private const string _errorMessage = "Error processing message. See inner exception for details.";
        
        public MessageReceivedException(Exception innerException) : base(_errorMessage, innerException) { }
    }
}
