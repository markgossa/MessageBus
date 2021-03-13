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
        private readonly string? _connectionString;
        private readonly string? _hostName;
        private string? _messageVersionPropertyName;
        private readonly ServiceBusAdministrationClient _serviceBusAdminClient;
        private readonly string? _tenantId;
        private readonly CreateSubscriptionOptions _createSubscriptionOptions;

        public AzureServiceBusAdminClient(string connectionString, string topic, string subscription)
        {
            _connectionString = connectionString;
            _createSubscriptionOptions = new CreateSubscriptionOptions(topic, subscription);
            _serviceBusAdminClient = BuildServiceBusAdminClient();
        }
        
        public AzureServiceBusAdminClient(string hostName, string topic, string subscription,
            string tenantId)
        {
            _hostName = hostName;
            _createSubscriptionOptions = new CreateSubscriptionOptions(topic, subscription);
            _tenantId = tenantId;
            _serviceBusAdminClient = BuildServiceBusAdminClient();
        }
        
        public AzureServiceBusAdminClient(string connectionString, CreateSubscriptionOptions createSubscriptionOptions)
        {
            _connectionString = connectionString;
            _createSubscriptionOptions = createSubscriptionOptions;
            ThrowIfInvalidCreateSubscriptionOptions(createSubscriptionOptions);
            _serviceBusAdminClient = BuildServiceBusAdminClient();
        }

        public AzureServiceBusAdminClient(string hostName, string tenantId, CreateSubscriptionOptions createSubscriptionOptions)
        {
            _hostName = hostName;
            _tenantId = tenantId;
            _createSubscriptionOptions = createSubscriptionOptions;
            ThrowIfInvalidCreateSubscriptionOptions(createSubscriptionOptions);
            _serviceBusAdminClient = BuildServiceBusAdminClient();
        }

        public async Task ConfigureAsync(IEnumerable<MessageHandlerMapping> messageHandlerMappings, 
            MessageBusOptions options)
        {
            _messageTypePropertyName = options?.MessageTypePropertyName;
            _messageVersionPropertyName = options?.MessageVersionPropertyName;
            await CreateOrUpdateSubscriptionAsync();
            await UpdateRulesAsync(messageHandlerMappings);
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var subscription = await _serviceBusAdminClient.GetSubscriptionAsync(_createSubscriptionOptions.TopicName, 
                    _createSubscriptionOptions.SubscriptionName);
                return subscription.Value != null;
            }
            catch
            {
                return false;
            }
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

        private static void ThrowIfInvalidCreateSubscriptionOptions(CreateSubscriptionOptions createSubscriptionOptions)
        {
            if (createSubscriptionOptions.RequiresSession)
            {
                throw new InvalidOperationException("RequiresSession is not yet supported");
            }
        }
        
        private async Task CreateOrUpdateSubscriptionAsync()
        {
            var subscriptionProperties = await GetSubscriptionAsync();
            if (subscriptionProperties is null)
            {
                await CreateSubscriptionAsync();
            }
            else if (SubscriptionSessionSettingsChanged(subscriptionProperties))
            {
                await DeleteSubscriptionAsync();
                await CreateSubscriptionAsync();
            }
            else
            {
                if (!SubscriptionOptionsAreCorrect(subscriptionProperties))
                {
                    await UpdateSubscriptionAsync(subscriptionProperties);
                }
            }
        }

        private async Task<SubscriptionProperties?> GetSubscriptionAsync()
        {
            SubscriptionProperties? subscription = null;
            try
            {
                subscription = (await _serviceBusAdminClient.GetSubscriptionAsync(_createSubscriptionOptions.TopicName,
                    _createSubscriptionOptions.SubscriptionName)).Value;
            }
            catch { }

            return subscription;
        }

        private async Task<Azure.Response<SubscriptionProperties>> CreateSubscriptionAsync()
            => await _serviceBusAdminClient.CreateSubscriptionAsync(_createSubscriptionOptions);

        private bool SubscriptionSessionSettingsChanged(SubscriptionProperties subscriptionProperties)
            => subscriptionProperties.RequiresSession != _createSubscriptionOptions.RequiresSession;
        
        private Task DeleteSubscriptionAsync()
            => _serviceBusAdminClient.DeleteSubscriptionAsync(_createSubscriptionOptions.TopicName,
                    _createSubscriptionOptions.SubscriptionName);

        private bool SubscriptionOptionsAreCorrect(SubscriptionProperties subscriptionProperties)
            => _createSubscriptionOptions == new CreateSubscriptionOptions(subscriptionProperties);
        
        private async Task UpdateSubscriptionAsync(SubscriptionProperties existingSubscriptionProperties)
        {
            var newSubscriptionProperties = CreateNewSubscriptionProperties(existingSubscriptionProperties);
            await _serviceBusAdminClient.UpdateSubscriptionAsync(newSubscriptionProperties);
        }

        private SubscriptionProperties CreateNewSubscriptionProperties(SubscriptionProperties subscriptionProperties)
        {
            subscriptionProperties.AutoDeleteOnIdle = _createSubscriptionOptions.AutoDeleteOnIdle;
            subscriptionProperties.DeadLetteringOnMessageExpiration = _createSubscriptionOptions.DeadLetteringOnMessageExpiration;
            subscriptionProperties.DefaultMessageTimeToLive = _createSubscriptionOptions.DefaultMessageTimeToLive;
            subscriptionProperties.EnableBatchedOperations = _createSubscriptionOptions.EnableBatchedOperations;
            subscriptionProperties.EnableDeadLetteringOnFilterEvaluationExceptions = _createSubscriptionOptions.EnableDeadLetteringOnFilterEvaluationExceptions;
            subscriptionProperties.ForwardDeadLetteredMessagesTo = _createSubscriptionOptions.ForwardDeadLetteredMessagesTo;
            subscriptionProperties.ForwardTo = _createSubscriptionOptions.ForwardTo;
            subscriptionProperties.LockDuration = _createSubscriptionOptions.LockDuration;
            subscriptionProperties.MaxDeliveryCount = _createSubscriptionOptions.MaxDeliveryCount;
            subscriptionProperties.RequiresSession = _createSubscriptionOptions.RequiresSession;

            return subscriptionProperties;
        }

        private async Task UpdateRulesAsync(IEnumerable<MessageHandlerMapping> messageHandlerMappings)
        {
            var newRules = BuildListOfNewRules(messageHandlerMappings);
            var existingRules = await GetExistingRulesAsync();

            await DeleteInvalidRulesAsync(newRules, existingRules);
            await AddNewRulesAsync(newRules, existingRules);
        }

        private List<CreateRuleOptions> BuildListOfNewRules(IEnumerable<MessageHandlerMapping> messageHandlerMappings)
        {
            var newRules = new List<CreateRuleOptions>();
            foreach (var messageHandlerMapping in messageHandlerMappings)
            {
                newRules.Add(MapMessageHandlerMappingToCorrelationRuleFilter(messageHandlerMapping));
            }

            return newRules;
        }

        private CreateRuleOptions MapMessageHandlerMappingToCorrelationRuleFilter(MessageHandlerMapping messageSubscription)
        {
            var filter = new CorrelationRuleFilter
            {
                Subject = messageSubscription.SubscriptionFilter!.Label
            };

            AddMessageFilterProperties(messageSubscription, messageSubscription.MessageType, filter);

            var newRule = new CreateRuleOptions(messageSubscription.MessageType.Name, filter);
            return newRule;
        }

        private async Task<List<RuleProperties>> GetExistingRulesAsync()
        {
            var existingRules = new List<RuleProperties>();
            await foreach (var existingRule in _serviceBusAdminClient.GetRulesAsync(_createSubscriptionOptions.TopicName,
                    _createSubscriptionOptions.SubscriptionName))
            {
                existingRules.Add(existingRule);
            }

            return existingRules;
        }

        private async Task DeleteInvalidRulesAsync(List<CreateRuleOptions> newRules, List<RuleProperties> existingRules)
        {
            foreach (var existingRule in existingRules.Where(e => !ExistingRuleIsValid(newRules, e)))
            {
                await _serviceBusAdminClient.DeleteRuleAsync(_createSubscriptionOptions.TopicName,
                    _createSubscriptionOptions.SubscriptionName, existingRule.Name);
            }
        }

        private async Task AddNewRulesAsync(List<CreateRuleOptions> newRules, List<RuleProperties> existingRules)
        {
            foreach (var newRule in newRules.Where(n => !NewRuleExists(existingRules, n)))
            {
                await _serviceBusAdminClient.CreateRuleAsync(_createSubscriptionOptions.TopicName,
                    _createSubscriptionOptions.SubscriptionName, newRule);
            }
        }

        private void AddMessageFilterProperties(MessageHandlerMapping messageHandlerMapping, Type messageType,
            CorrelationRuleFilter filter)
        {
            if (messageHandlerMapping.SubscriptionFilter?.MessageProperties.Count > 0)
            {
                AddCustomMessageProperties(messageHandlerMapping, filter);
            }
            else
            {
                AddMessageVersionProperty(messageType, filter);
            }
        }

        private static void AddCustomMessageProperties(MessageHandlerMapping messageHandlerMapping, CorrelationRuleFilter filter)
        {
            if (messageHandlerMapping.SubscriptionFilter != null)
            {
                foreach (var property in messageHandlerMapping.SubscriptionFilter.MessageProperties)
                {
                    filter.ApplicationProperties.Add(property.Key, property.Value);
                }
            }
        }

        private void AddMessageVersionProperty(Type messageType, CorrelationRuleFilter filter)
        {
            var messageVersion = messageType.GetCustomAttribute<MessageVersionAttribute>();
            if (messageVersion != null)
            {
                filter.ApplicationProperties.Add(_messageVersionPropertyName, messageVersion.Version);
            }
        }

        private static bool ExistingRuleIsValid(List<CreateRuleOptions> newRules, RuleProperties existingRule) => 
            newRules.Any(r => r.Name == existingRule.Name && r.Filter == existingRule.Filter);

        private static bool NewRuleExists(List<RuleProperties> existingRules, CreateRuleOptions newRule) 
            => existingRules.Any(r => r.Name == newRule.Name && r.Filter == newRule.Filter);
    }
}
