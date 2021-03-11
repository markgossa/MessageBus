using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public interface IMessageHandlerResolver
    {
        void SubcribeToMessage<TMessage, TMessageHandler>(string messageType, SubscriptionFilter? subscriptionFilter = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>;

        object Resolve(string messageType);
        IEnumerable<MessageSubscription> GetMessageSubscriptions();
        void Initialize();
    }
}