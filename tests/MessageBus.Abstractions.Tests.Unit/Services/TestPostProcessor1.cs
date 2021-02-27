using System.Threading.Tasks;

namespace MessageBus.Abstractions.Tests.Unit
{
    internal class TestPostProcessor1 : IMessagePostProcessor
    {
        public Task ProcessAsync(IMessageContext<IMessage> context) => throw new System.NotImplementedException();
    }
}