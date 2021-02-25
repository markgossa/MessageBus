using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageProcessorResolver
    {
        private readonly ServiceCollection _services;
        private ServiceProvider? _serviceProvider;
        private readonly List<IMessageProcessor> _messageProcessors = new List<IMessageProcessor>();

        public MessageProcessorResolver(ServiceCollection services)
        {
            _services = services;
        }

        public void Initialize()
        {
            _serviceProvider = _services.BuildServiceProvider();
            
            if (_serviceProvider != null)
            {
                _messageProcessors.AddRange(_serviceProvider.GetServices<IMessagePreProcessor>());
                _messageProcessors.AddRange(_serviceProvider.GetServices<IMessagePostProcessor>());
            }
        }

        public object Resolve<T>() where T : notnull, IMessageProcessor 
            => _messageProcessors.FirstOrDefault(p => p is T)
                ?? throw new MessageProcessorNotFound($"Messsage processor {typeof(T).Name} not found");
    }
}
