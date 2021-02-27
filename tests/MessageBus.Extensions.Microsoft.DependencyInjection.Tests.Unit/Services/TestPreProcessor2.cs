using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services
{
    internal class TestPreProcessor2 : IMessagePreProcessor
    {
        public Task ProcessAsync<T>(IMessageContext<T> context) where T : IMessage => throw new System.NotImplementedException();
    }
}