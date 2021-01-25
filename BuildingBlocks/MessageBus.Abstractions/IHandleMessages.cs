using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IHandleMessages<T> where T : IMessage
    {
        Task HandleAsync(MessageContext<T> context);
    }
}