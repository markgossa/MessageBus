using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public interface IMessageProcessorResolver
    {
        void AddMessagePreProcessor<T>() where T : class, IMessagePreProcessor;
        void AddMessagePostProcessor<T>() where T : class, IMessagePostProcessor;
        IEnumerable<IMessagePreProcessor> GetMessagePreProcessors();
        void Initialize();
    }
}
