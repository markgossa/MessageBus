using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusReceiver
    {
        Task ConfigureAsync();
        Task StartAsync();
    }
}