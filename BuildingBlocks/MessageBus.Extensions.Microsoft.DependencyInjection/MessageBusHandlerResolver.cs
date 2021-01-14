using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageBusHandlerResolver
    {
        private readonly ServiceProvider _serviceProvider;
        public readonly Dictionary<Type, ServiceDescriptor> _handlerMap;

        public MessageBusHandlerResolver(ServiceCollection services)
        {
            _serviceProvider = services.BuildServiceProvider();
            _handlerMap = GetMessageBusHandlerServiceDescriptors(services).ToDictionary(h => GetMessageTypeFromHandler(h.ImplementationType),
                h => h);
        }

        public object Resolve(Type messageType)
        {
            var handlerServiceType = _handlerMap.First(h => h.Key == messageType).Value.ServiceType;
            return _serviceProvider.GetRequiredService(handlerServiceType);
        }

        private static Type GetMessageTypeFromHandler(Type handler)
            => handler.GetInterfaces()
                .First(i => i.Name.Contains(typeof(IHandleMessages<>).Name))
                .GenericTypeArguments.First();

        public IEnumerable<ServiceDescriptor> GetMessageBusHandlerServiceDescriptors(ServiceCollection services)
            => services.AsEnumerable()
                .Where(s => s.ServiceType.FullName.Contains(typeof(IHandleMessages<>).FullName)
                    && s.ServiceType.Assembly.FullName.Contains(typeof(IHandleMessages<>).Assembly.FullName));
    }
}
