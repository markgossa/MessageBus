using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services
{
    internal class TestPostProcessor1 : IMessagePostProcessor
    {
        public Task ProcessAsync<T>(IMessageContext<T> context) where T : IMessage => throw new System.NotImplementedException();
    }
}
