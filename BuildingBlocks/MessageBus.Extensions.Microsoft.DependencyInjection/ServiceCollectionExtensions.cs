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
            services.AddScoped(typeof(IMessageHandler<>).MakeGenericType(eventType), handlerType);
            return services;
        }

        public static IServiceCollection SubscribeToMessage<TMessage, TMessageHandler>(this IServiceCollection services)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>
        {
            services.AddScoped(typeof(IMessageHandler<>).MakeGenericType(typeof(TMessage)), typeof(TMessageHandler));
            return services;
        }

        public static IServiceCollection AddMessageBus(this IServiceCollection services, IMessageBusAdminClient messageBusAdmin,
            IMessageBusClient messageBusClient)
        {
            services.AddSingleton((IMessageBus)new Abstractions.MessageBus(new MessageBusHandlerResolver(services),
                messageBusAdmin, messageBusClient));

            return services;
        }
        
        public static async Task<IServiceCollection> AddMessageBusAsync(this IServiceCollection services, IMessageBusClientBuilder messageBusClientBuilder)
        {
            AddMessageBus(services, await messageBusClientBuilder.BuildMessageBusAdminClientAsync(), 
                await messageBusClientBuilder.BuildMessageBusClientAsync());

            return services;
        }
        
        public static IServiceCollection AddMessageBus(this IServiceCollection services, IMessageBusClientBuilder messageBusClientBuilder)
        {
            AddMessageBus(services, messageBusClientBuilder.BuildMessageBusAdminClientAsync().Result, 
                messageBusClientBuilder.BuildMessageBusClientAsync().Result);

            return services;
        }
    }
}
