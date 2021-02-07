using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public interface IMessageBusHandlerResolver
    {
        void SubcribeToMessage<TMessage, TMessageHandler>(Dictionary<string, string> messageProperties = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>;

        object Resolve(string messageType);
        IEnumerable<MessageSubscription> GetMessageSubscriptions();
        void Initialize();
    }
}