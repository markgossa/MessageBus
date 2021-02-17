using MessageBus.Microsoft.ServiceBus;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class AzureServiceBusClientBuilderTests
    {
        protected const string _connectionString = "Endpoint=sb://testsb.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=testsharedaccesskey;";
        private const string _hostname = "testsb.servicebus.windows.net";
        protected const string _topic = "topic1";
        protected readonly string _subscription = nameof(AzureServiceBusClientBuilderTests);
        private const string _tenantId = "12345";

        [Fact]
        public async Task BuildsServiceBusClientWithConnectionString()
        {
            var sut = new AzureServiceBusClientBuilder(_connectionString, _topic, _subscription);
            var messageBusClient = await sut.BuildMessageBusClientAsync();

            Assert.IsAssignableFrom<IMessageBusClient>(messageBusClient);
            Assert.IsAssignableFrom<AzureServiceBusClient>(messageBusClient);
        }
        
        [Fact]
        public async Task BuildsServiceBusClientWithManagedIdentity()
        {
            var sut = new AzureServiceBusClientBuilder(_hostname, _topic, _subscription, _tenantId);
            var messageBusClient = await sut.BuildMessageBusClientAsync();

            Assert.IsAssignableFrom<IMessageBusClient>(messageBusClient);
            Assert.IsAssignableFrom<AzureServiceBusClient>(messageBusClient);
        }

        [Fact]
        public async Task BuildsServiceBusAdminClientWithConnectionString()
        {
            var sut = new AzureServiceBusClientBuilder(_connectionString, _topic, _subscription);
            var messageBusAdminClient = await sut.BuildMessageBusAdminClientAsync();

            Assert.IsAssignableFrom<IMessageBusAdminClient>(messageBusAdminClient);
            Assert.IsAssignableFrom<AzureServiceBusAdminClient>(messageBusAdminClient);
        }
        
        [Fact]
        public async Task BuildsServiceBusAdminClientWithManagedIdentity()
        {
            var sut = new AzureServiceBusClientBuilder(_hostname, _topic, _subscription, _tenantId);
            var messageBusAdminClient = await sut.BuildMessageBusAdminClientAsync();

            Assert.IsAssignableFrom<IMessageBusAdminClient>(messageBusAdminClient);
            Assert.IsAssignableFrom<AzureServiceBusAdminClient>(messageBusAdminClient);
        }
    }
}
