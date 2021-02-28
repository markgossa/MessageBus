using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageProcessor
    {
        Task ProcessAsync<T>(IMessageContext<T> context) where T : IMessage;
    }
}