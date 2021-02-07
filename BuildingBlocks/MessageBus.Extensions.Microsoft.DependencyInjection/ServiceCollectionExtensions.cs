using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IMessageBus AddMessageBus(this IServiceCollection services, IMessageBusAdminClient messageBusAdmin, 
            IMessageBusClient messageBusClient)
        {
            var messageBus = (IMessageBus)new Abstractions.MessageBus(new MessageBusHandlerResolver(services),
                messageBusAdmin, messageBusClient);
            services.AddSingleton(messageBus);

            return messageBus;
        }

        public static IMessageBus AddMessageBus(this IServiceCollection services, IMessageBusClientBuilder messageBusClientBuilder) 
            => AddMessageBus(services, messageBusClientBuilder.BuildMessageBusAdminClientAsync().Result,
                messageBusClientBuilder.BuildMessageBusClientAsync().Result);
    }
}
