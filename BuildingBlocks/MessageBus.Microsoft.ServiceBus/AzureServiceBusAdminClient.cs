using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus
{
    public class AzureServiceBusAdminClient : IMessageBusAdminClient
    {
        private readonly string _connectionString;
        private readonly string _topic;
        private readonly string _subscription;
        private readonly string _messageTypePropertyName;
        private readonly ServiceBusAdministrationClient _serviceBusAdminClient;
        private readonly string _tenantId;
        private IEnumerable<Type> _handlers;

        public AzureServiceBusAdminClient(string connectionString, string topic, string subscription,
            string tenantId = null, string messageTypePropertyName = "MessageType")
        {
            _connectionString = connectionString;
            _topic = topic;
            _subscription = subscription;
            _tenantId = tenantId;
            _messageTypePropertyName = messageTypePropertyName;
            _serviceBusAdminClient = BuildServiceBusAdminClient();
        }

        private ServiceBusAdministrationClient BuildServiceBusAdminClient()
            => string.IsNullOrEmpty(_tenantId)
                ? new ServiceBusAdministrationClient(_connectionString)
                : new ServiceBusAdministrationClient(_connectionString, new ServiceBusTokenProvider(_tenantId));

        public async Task ConfigureAsync(IEnumerable<Type> messageHandlers)
        {
            await CreateSubscriptionAsync();
            await RemoveAllRulesAsync();
            await AddRulesAsync(messageHandlers);
        }

        private async Task CreateSubscriptionAsync()
        {
            SubscriptionProperties subscription = null;
            try
            {
                subscription = (await _serviceBusAdminClient.GetSubscriptionAsync(_topic, _subscription)).Value;
            }
            catch (Exception) {}

            if (subscription is null)
            {
                await _serviceBusAdminClient.CreateSubscriptionAsync(_topic, _subscription);
            }
        }

        private async Task AddRulesAsync(IEnumerable<Type> messageHandlers)
        {
            _handlers = messageHandlers;
            foreach (var handler in _handlers)
            {
                await AddRuleAsync(_serviceBusAdminClient, GetMessageTypeFromHandler(handler));
            }
        }

        private async Task RemoveAllRulesAsync()
        {
            var rules = _serviceBusAdminClient.GetRulesAsync(_topic, _subscription);
            await foreach (var rule in rules)
            {
                await _serviceBusAdminClient.DeleteRuleAsync(_topic, _subscription, rule.Name);
            }
        }

        private static Type GetMessageTypeFromHandler(Type handler)
            => handler
                .GetInterfaces()
                .First(i => i.Name.Contains(typeof(IMessageHandler<>).Name))
                .GenericTypeArguments.First();

        private async Task AddRuleAsync(ServiceBusAdministrationClient client, Type messageType)
        {
            var filter = new CorrelationRuleFilter();
            filter.ApplicationProperties.Add(_messageTypePropertyName, messageType.Name);
            await client.CreateRuleAsync(_topic, _subscription, new CreateRuleOptions(messageType.Name, filter));
        }
    }
}
