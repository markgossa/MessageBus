using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBus
    {
        Task ConfigureAsync();
        Task StartAsync();
        Task DeadLetterMessageAsync(object message, string? reason = null);
    }
}