using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusReceiver
    {
        Task ConfigureAsync();
        Task StartAsync();
        Task DeadLetterMessageAsync(object message, string? reason = null);
    }
}