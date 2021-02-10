using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus
{
    public class AzureServiceBusClientBuilder : IMessageBusClientBuilder
    {
        private readonly string? _connectionString;
        private readonly string? _hostname;
        private readonly string _topic;
        private readonly string _subscription;
        private readonly string? _tenantId;

        public AzureServiceBusClientBuilder(string connectionString, string topic, string subscription)
        {
            _connectionString = connectionString;
            _topic = topic;
            _subscription = subscription;
        }

        public AzureServiceBusClientBuilder(string hostname, string topic, string subscription, string tenantId)
        {
            _hostname = hostname;
            _topic = topic;
            _subscription = subscription;
            _tenantId = tenantId;
        }

        public async Task<IMessageBusClient> BuildMessageBusClientAsync()
            => await Task.FromResult(string.IsNullOrEmpty(_tenantId)
                ? new AzureServiceBusClient(_connectionString, _topic, _subscription)
                : new AzureServiceBusClient(_hostname, _topic, _subscription, _tenantId));

        public async Task<IMessageBusAdminClient> BuildMessageBusAdminClientAsync()
            => await Task.FromResult(string.IsNullOrEmpty(_tenantId)
                ? new AzureServiceBusAdminClient(_connectionString, _topic, _subscription)
                : new AzureServiceBusAdminClient(_hostname, _topic, _subscription, _tenantId));
    }
}
