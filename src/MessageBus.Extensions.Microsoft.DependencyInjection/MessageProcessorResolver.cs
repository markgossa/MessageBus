using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageProcessorResolver : IMessageProcessorResolver
    {
        private readonly IServiceCollection _services;
        private IServiceProvider? _serviceProvider;

        public IEnumerable<IMessagePreProcessor> GetMessagePreProcessors() 
            => GetMessageProcessors<IMessagePreProcessor>();

        public IEnumerable<IMessagePostProcessor> GetMessagePostProcessors() 
            => GetMessageProcessors<IMessagePostProcessor>();

        public MessageProcessorResolver(IServiceCollection services)
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
