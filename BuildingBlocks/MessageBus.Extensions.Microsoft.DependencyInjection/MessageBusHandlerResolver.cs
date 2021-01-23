using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageBusHandlerResolver : IMessageBusHandlerResolver
    {
        private readonly ServiceProvider _serviceProvider;
        public Dictionary<Type, ServiceDescriptor> _handlerMap;

        public MessageBusHandlerResolver(IServiceCollection services)
        {
            _serviceProvider = services.BuildServiceProvider();
            _handlerMap = GetMessageBusHandlerServiceDescriptors(services).ToDictionary(h => 
                GetMessageTypeFromHandler(h.ImplementationType), h => h);
        }

        public object Resolve(string messageType)
        {
            var handlerServiceType = _handlerMap.FirstOrDefault(h => h.Key.Name == messageType).Value?.ServiceType;
            ThrowIfMessageHandlerNotFound(messageType, handlerServiceType);

            return _serviceProvider.GetRequiredService(handlerServiceType);
        }

        public IEnumerable<Type> GetMessageHandlers() => _handlerMap.Values.Select(h => h.ImplementationType);

        private static Type GetMessageTypeFromHandler(Type handler)
            => handler.GetInterfaces()
                .First(i => i.Name.Contains(typeof(IHandleMessages<>).Name))
                .GenericTypeArguments.First();

        private static IEnumerable<ServiceDescriptor> GetMessageBusHandlerServiceDescriptors(IServiceCollection services)
            => services.AsEnumerable()
                .Where(s => s.ServiceType.FullName.Contains(typeof(IHandleMessages<>).FullName)
                    && s.ServiceType.Assembly.FullName.Contains(typeof(IHandleMessages<>).Assembly.FullName));

        private static void ThrowIfMessageHandlerNotFound(string messageType, Type handlerServiceType)
        {
            if (handlerServiceType is null)
            {
                throw new MessageHandlerNotFoundException($"Message handler for message type {messageType} was not found");
            }
        }
    }
}
