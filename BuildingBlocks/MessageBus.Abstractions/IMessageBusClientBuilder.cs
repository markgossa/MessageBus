using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusClientBuilder
    {
        Task<IMessageBusClient> BuildMessageBusClientAsync();
        Task<IMessageBusAdminClient> BuildMessageBusAdminClientAsync();
    }
}
