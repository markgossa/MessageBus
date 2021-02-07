using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus
{
    public class AzureServiceBusAdminClient : IMessageBusAdminClient
    {
        private const string _defaultMessageTypePropertyName = "MessageType";
        private const string _defaultMessageVersionPropertyName = "MessageVersion";
        private readonly string _connectionString;
        private readonly string _hostName;
        private readonly string _topic;
        private readonly string _subscription;
        private readonly string _messageTypePropertyName;
        private readonly string _messageVersionPropertyName;
        private readonly ServiceBusAdministrationClient _serviceBusAdminClient;
        private readonly string _tenantId;

        public AzureServiceBusAdminClient(string connectionString, string topic, string subscription,
            AzureServiceBusAdminClientOptions options = null)
        {
            _connectionString = connectionString;
            _topic = topic;
            _subscription = subscription;
            _messageTypePropertyName = options?.MessageTypePropertyName ?? _defaultMessageTypePropertyName;
            _messageVersionPropertyName = options?.MessageVersionPropertyName ?? _defaultMessageVersionPropertyName;
            _serviceBusAdminClient = BuildServiceBusAdminClient();
        }
        
        public AzureServiceBusAdminClient(string hostName, string topic, string subscription,
            string tenantId, AzureServiceBusAdminClientOptions options = null)
        {
            _hostName = hostName;
            _topic = topic;
            _subscription = subscription;
            _tenantId = tenantId;
            _messageTypePropertyName = options?.MessageTypePropertyName ?? _defaultMessageTypePropertyName;
            _messageVersionPropertyName = options?.MessageVersionPropertyName ?? _defaultMessageVersionPropertyName;
            _serviceBusAdminClient = BuildServiceBusAdminClient();
        }

        private ServiceBusAdministrationClient BuildServiceBusAdminClient()
        {
            var options = new DefaultAzureCredentialOptions
            {
                SharedTokenCacheTenantId = _tenantId
            };

            return string.IsNullOrEmpty(_tenantId)
                ? new ServiceBusAdministrationClient(_connectionString)
                : new ServiceBusAdministrationClient(_hostName, new DefaultAzureCredential(options));
        }

        public async Task ConfigureAsync(IEnumerable<MessageSubscription> messageSubscriptions)
        {
            await CreateSubscriptionAsync();
            await UpdateRulesAsync(messageSubscriptions);
        }

        private async Task CreateSubscriptionAsync()
        {
            if (!await SubscriptionExistsAsync())
            {
                await _serviceBusAdminClient.CreateSubscriptionAsync(_topic, _subscription);
            }
        }

        private async Task UpdateRulesAsync(IEnumerable<MessageSubscription> messageSubscriptions)
        {
            var newRules = BuildListOfNewRules(messageSubscriptions.Select(m => m.MessageHandlerType));
            var existingRules = await GetExistingRulesAsync();

            await DeleteInvalidRulesAsync(newRules, existingRules);
            await AddNewRulesAsync(newRules, existingRules);
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var subscription = await _serviceBusAdminClient.GetSubscriptionAsync(_topic, _subscription);
                return subscription.Value is not null;
            }
            catch
            {
                return false;
            }
        }

        private List<CreateRuleOptions> BuildListOfNewRules(IEnumerable<Type> messageHandlers)
        {
            var newRules = new List<CreateRuleOptions>();
            foreach (var messageHandler in messageHandlers)
            {
                var messageType = GetMessageTypeFromHandler(messageHandler);
                var filter = new CorrelationRuleFilter();

                AddMessageTypeProperty(messageType, filter);
                AddMessageVersionProperty(messageType, filter);

                newRules.Add(new CreateRuleOptions(messageType.Name, filter));
            }

            return newRules;
        }

        private async Task<List<RuleProperties>> GetExistingRulesAsync()
        {
            var existingRules = new List<RuleProperties>();
            await foreach (var existingRule in _serviceBusAdminClient.GetRulesAsync(_topic, _subscription))
            {
                existingRules.Add(existingRule);
            }

            return existingRules;
        }

        private async Task DeleteInvalidRulesAsync(List<CreateRuleOptions> newRules, List<RuleProperties> existingRules)
        {
            foreach (var existingRule in existingRules.Where(e => !ExistingRuleIsValid(newRules, e)))
            {
                await _serviceBusAdminClient.DeleteRuleAsync(_topic, _subscription, existingRule.Name);
            }
        }

        private async Task AddNewRulesAsync(List<CreateRuleOptions> newRules, List<RuleProperties> existingRules)
        {
            foreach (var newRule in newRules.Where(n => !NewRuleExists(existingRules, n)))
            {
                await _serviceBusAdminClient.CreateRuleAsync(_topic, _subscription, newRule);
            }
        }

        private void AddMessageTypeProperty(Type messageType, CorrelationRuleFilter filter)
            => filter.ApplicationProperties.Add(_messageTypePropertyName, messageType.Name);

        private void AddMessageVersionProperty(Type messageType, CorrelationRuleFilter filter)
        {
            var messageVersion = messageType.GetCustomAttribute<MessageVersionAttribute>();
            if (messageVersion is not null)
            {
                filter.ApplicationProperties.Add(_messageVersionPropertyName, messageVersion.Version);
            }
        }

        private async Task<bool> SubscriptionExistsAsync()
        {
            SubscriptionProperties subscription = null;
            try
            {
                subscription = (await _serviceBusAdminClient.GetSubscriptionAsync(_topic, _subscription)).Value;
            }
            catch { }
            var subscriptionExists = subscription is not null;
            return subscriptionExists;
        }

        private static Type GetMessageTypeFromHandler(Type handler)
            => handler
                .GetInterfaces()
                .First(i => i.Name.Contains(typeof(IMessageHandler<>).Name))
                .GenericTypeArguments.First();

        private static bool ExistingRuleIsValid(List<CreateRuleOptions> newRules, RuleProperties existingRule) => 
            newRules.Any(r => r.Name == existingRule.Name && r.Filter == existingRule.Filter);

        private static bool NewRuleExists(List<RuleProperties> existingRules, CreateRuleOptions newRule) 
            => existingRules.Any(r => r.Name == newRule.Name && r.Filter == newRule.Filter);
    }
}
