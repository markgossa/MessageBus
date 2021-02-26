using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IMessageBus AddMessageBus(this IServiceCollection services, IMessageBusAdminClient messageBusAdmin, 
            IMessageBusClient messageBusClient, MessageBusOptions? options = null)
        {
            var messageBus = (IMessageBus)new Abstractions.MessageBus(new MessageHandlerResolver(services),
                messageBusAdmin, messageBusClient, new MessageProcessorResolver(services), options);
            services.AddSingleton(messageBus);

            return messageBus;
        }

        public static IMessageBus AddMessageBus(this IServiceCollection services, IMessageBusClientBuilder messageBusClientBuilder, 
            MessageBusOptions? options = null) 
            => AddMessageBus(services, messageBusClientBuilder.BuildMessageBusAdminClientAsync().Result,
                messageBusClientBuilder.BuildMessageBusClientAsync().Result, options);
    }
}
