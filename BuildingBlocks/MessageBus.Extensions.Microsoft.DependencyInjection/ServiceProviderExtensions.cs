using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

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
            where TMessage : IEvent
            where TMessageHandler : IHandleMessages<TMessage>
        {
            services.AddScoped(typeof(IHandleMessages<>).MakeGenericType(typeof(TMessage)), typeof(TMessageHandler));
            return services;
        }

        public static ServiceCollection AddMessageBus(this ServiceCollection services, IMessageBusAdmin messageBusAdmin,
            IMessageBusClient messageBusClient)
        {
            //messageBusAdmin.ConfigureAsync(services.GetMessageBusHandlers()).Wait();
            services.AddSingleton<IMessageBusService>(new MessageBusService(new MessageBusHandlerResolver(services),
                messageBusAdmin, messageBusClient));

            return services;
        }

        private static IEnumerable<Type> GetMessageBusHandlers(this ServiceCollection services)
            => services.AsEnumerable()
                .Where(s => s.ServiceType.FullName.Contains(typeof(IHandleMessages<>).FullName)
                    && s.ServiceType.Assembly.FullName.Contains(typeof(IHandleMessages<>).Assembly.FullName))
                .Select(s => s.ImplementationType);
    }
}
