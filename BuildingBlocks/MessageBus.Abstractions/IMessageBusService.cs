using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusService
    {
        Task ConfigureAsync();
        Task StartAsync();
    }
}