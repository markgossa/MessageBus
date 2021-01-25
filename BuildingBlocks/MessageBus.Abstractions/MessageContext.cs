using System;
using System.Text.Json;

namespace MessageBus.Abstractions
{
    public class MessageContext<T> where T : IMessage
    {
        public BinaryData Body { get; internal set; }
        public T Message => JsonSerializer.Deserialize<T>(Body.ToString());
    }
}
