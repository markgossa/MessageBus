using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageHandlerResolverTestsBase
    {
        protected static SubscriptionFilter BuildSubscriptionFilter<T>() where T : IMessage
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                Label = typeof(T).Name
            };

            subscriptionFilter.Build(new MessageBusOptions(), typeof(T));

            return subscriptionFilter;
        }
    }
}