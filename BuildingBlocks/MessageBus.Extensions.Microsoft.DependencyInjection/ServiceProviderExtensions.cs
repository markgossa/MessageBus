using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static ServiceCollection SubscribeToMessage(this ServiceCollection services, Type eventType,
            Type handlerType)
        {
            services.AddScoped(typeof(IHandleMessages<>).MakeGenericType(eventType), handlerType);
            return services;
        }

        public static ServiceCollection SubscribeToMessage<TMessage, TMessageHandler>(this ServiceCollection services)
            where TMessage : IMessage
            where TMessageHandler : IHandleMessages<TMessage>
        {
            services.AddScoped(typeof(IHandleMessages<>).MakeGenericType(typeof(TMessage)), typeof(TMessageHandler));
            return services;
        }

        public static ServiceCollection AddMessageBusReceiver(this ServiceCollection services, IMessageBusAdminClient messageBusAdmin,
            IMessageBusClient messageBusClient)
        {
            services.AddSingleton<IMessageBusReceiver>(new MessageBusReceiver(new MessageBusHandlerResolver(services),
                messageBusAdmin, messageBusClient));

            return services;
        }
        
        public static async Task<ServiceCollection> AddMessageBusReceiverAsync(this ServiceCollection services, IMessageBusClientBuilder messageBusClientBuilder)
        {
            AddMessageBusReceiver(services, await messageBusClientBuilder.BuildMessageBusAdminClientAsync(), 
                await messageBusClientBuilder.BuildMessageBusClientAsync());

            return services;
        }
        
        public static ServiceCollection AddMessageBusReceiver(this ServiceCollection services, IMessageBusClientBuilder messageBusClientBuilder)
        {
            AddMessageBusReceiver(services, messageBusClientBuilder.BuildMessageBusAdminClientAsync().Result, 
                messageBusClientBuilder.BuildMessageBusClientAsync().Result);

            return services;
        }
    }
}
