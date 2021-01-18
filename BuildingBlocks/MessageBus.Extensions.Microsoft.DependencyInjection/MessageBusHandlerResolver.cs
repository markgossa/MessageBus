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

        public MessageBusHandlerResolver(ServiceCollection services)
        {
            _serviceProvider = services.BuildServiceProvider();
            _handlerMap = GetMessageBusHandlerServiceDescriptors(services).ToDictionary(h => GetMessageTypeFromHandler(h.ImplementationType),
                h => h);
        }

        public object Resolve(string messageType)
        {
            var handlerServiceType = _handlerMap.First(h => h.Key.Name == messageType).Value.ServiceType;
            
            return _serviceProvider.GetRequiredService(handlerServiceType);
        }

        public IEnumerable<Type> GetMessageHandlers() => _handlerMap.Values.Select(h => h.ImplementationType);

        private static Type GetMessageTypeFromHandler(Type handler)
            => handler.GetInterfaces()
                .First(i => i.Name.Contains(typeof(IHandleMessages<>).Name))
                .GenericTypeArguments.First();

        private static IEnumerable<ServiceDescriptor> GetMessageBusHandlerServiceDescriptors(ServiceCollection services)
            => services.AsEnumerable()
                .Where(s => s.ServiceType.FullName.Contains(typeof(IHandleMessages<>).FullName)
                    && s.ServiceType.Assembly.FullName.Contains(typeof(IHandleMessages<>).Assembly.FullName));
    }
}
