using MessageBus.Abstractions;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageHandlerResolverTestsBase
    {
        protected static SubscriptionFilter BuildSubscriptionFilter<T>() where T : IMessage
        {
            var subscriptionFilter = new SubscriptionFilter();
            subscriptionFilter.Build(new MessageBusOptions(), typeof(T));

            return subscriptionFilter;
        }
    }
}