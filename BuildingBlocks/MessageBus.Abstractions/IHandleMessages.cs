using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IHandleMessages<T>
    {
        Task HandleAsync(T message);
    }
}