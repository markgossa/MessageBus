using MessageBus.Abstractions.Messages;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public interface IMessageHandlerResolver
    {
        void SubcribeToMessage<TMessage, TMessageHandler>(SubscriptionFilter subscriptionFilter = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>;

        object Resolve(string messageType);
        IEnumerable<MessageHandlerMapping> GetMessageHandlerMappings();
        void Initialize();
    }
}