using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBus
    {
        Task ConfigureAsync();
        Task StartAsync();
        Task StopAsync();
        Task DeadLetterMessageAsync(object message, string? reason = null);
        Task<bool> CheckHealthAsync();
    }
}