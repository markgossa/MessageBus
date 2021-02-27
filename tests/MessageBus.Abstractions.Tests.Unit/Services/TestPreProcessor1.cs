using System.Threading.Tasks;

namespace MessageBus.Abstractions.Tests.Unit.Services
{
    internal class TestPreProcessor1 : IMessagePreProcessor
    {
        public Task ProcessAsync(IMessageContext<IMessage> context) => throw new System.NotImplementedException();
    }
}