using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageProcessor
    {
        Task ProcessAsync(IMessageContext<IMessage> context);
    }
}