using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SubscribeToMessage(this IServiceCollection services, Type eventType,
            Type handlerType)
        {
            services.AddScoped(typeof(IHandleMessages<>).MakeGenericType(eventType), handlerType);
            return services;
        }

        public static IServiceCollection SubscribeToMessage<TMessage, TMessageHandler>(this IServiceCollection services)
            where TMessage : IMessage
            where TMessageHandler : IHandleMessages<TMessage>
        {
            services.AddScoped(typeof(IHandleMessages<>).MakeGenericType(typeof(TMessage)), typeof(TMessageHandler));
            return services;
        }

        public static IServiceCollection AddMessageBusReceiver(this IServiceCollection services, IMessageBusAdminClient messageBusAdmin,
            IMessageBusClient messageBusClient)
        {
            services.AddSingleton<IMessageBusReceiver>(new MessageBusReceiver(new MessageBusHandlerResolver(services),
                messageBusAdmin, messageBusClient));

            return services;
        }
        
        public static async Task<IServiceCollection> AddMessageBusReceiverAsync(this IServiceCollection services, IMessageBusClientBuilder messageBusClientBuilder)
        {
            AddMessageBusReceiver(services, await messageBusClientBuilder.BuildMessageBusAdminClientAsync(), 
                await messageBusClientBuilder.BuildMessageBusClientAsync());

            return services;
        }
        
        public static IServiceCollection AddMessageBusReceiver(this IServiceCollection services, IMessageBusClientBuilder messageBusClientBuilder)
        {
            AddMessageBusReceiver(services, messageBusClientBuilder.BuildMessageBusAdminClientAsync().Result, 
                messageBusClientBuilder.BuildMessageBusClientAsync().Result);

            return services;
        }
    }
}
