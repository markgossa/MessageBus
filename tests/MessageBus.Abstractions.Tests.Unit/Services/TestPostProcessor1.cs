using System.Threading.Tasks;

namespace MessageBus.Abstractions.Tests.Unit
{
    internal class TestPostProcessor1 : IMessagePostProcessor
    {
        public Task ProcessAsync<T>(IMessageContext<T> context) where T : IMessage => throw new System.NotImplementedException();
    }
}