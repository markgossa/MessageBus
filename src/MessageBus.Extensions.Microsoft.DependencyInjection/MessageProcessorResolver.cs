using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageProcessorResolver
    {
        private readonly ServiceCollection _services;
        private ServiceProvider? _serviceProvider;

        public IEnumerable<IMessagePreProcessor> PreProcessors
            => GetMessageProcessors<IMessagePreProcessor>();

        public IEnumerable<IMessagePostProcessor> PostProcessors
            => GetMessageProcessors<IMessagePostProcessor>();

        public MessageProcessorResolver(ServiceCollection services)
        {
            _services = services;
        }

        public void Initialize() => _serviceProvider = _services.BuildServiceProvider();

        public void AddMessagePreProcessor<T>() where T : class, IMessagePreProcessor 
            => _services.AddSingleton<IMessagePreProcessor, T>();
        
        public void AddMessagePostProcessor<T>() where T : class, IMessagePostProcessor
            => _services.AddSingleton<IMessagePostProcessor, T>();

        private IEnumerable<T> GetMessageProcessors<T>() where T : IMessageProcessor
            => _serviceProvider != null
                ? _serviceProvider.GetServices<T>()
                : throw new ApplicationException("Service Provider is null");
    }
}
