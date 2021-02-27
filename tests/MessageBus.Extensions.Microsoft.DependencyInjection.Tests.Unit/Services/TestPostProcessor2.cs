using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services
{
    internal class TestPostProcessor2 : IMessagePostProcessor
    {
        public Task ProcessAsync(IMessageContext<IMessage> context) => throw new System.NotImplementedException();
    }
}
