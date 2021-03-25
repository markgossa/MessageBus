using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services
{
    internal class TestPreProcessor1 : IMessagePreProcessor
    {
        public Task ProcessAsync<T>(IMessageContext<T> context) where T : IMessage => throw new System.NotImplementedException();
    }
}