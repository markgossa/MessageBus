using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusProcessor
    {
        Task StartAsync();
    }
}