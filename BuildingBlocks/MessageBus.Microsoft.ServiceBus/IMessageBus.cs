using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus
{
    public interface IMessageBus
    {
        Task StartAsync(ServiceCollection services);
    }
}