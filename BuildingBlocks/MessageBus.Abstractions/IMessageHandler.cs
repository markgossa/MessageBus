using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageHandler<T> where T : IMessage
    {
        Task HandleAsync(IMessageContext<T> context);
    }
}