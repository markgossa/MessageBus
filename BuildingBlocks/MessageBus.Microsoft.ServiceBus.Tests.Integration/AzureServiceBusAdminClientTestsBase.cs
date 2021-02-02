using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusAdminClientTestsBase
    {
        protected const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        protected const string _topic = "topic1";
        protected readonly string _subscription = nameof(AzureServiceBusAdminClientTestsBase);
        protected readonly ServiceBusClient _serviceBusClient = new ServiceBusClient(_connectionString);
        protected readonly ServiceBusAdministrationClient _serviceBusAdminClient = new ServiceBusAdministrationClient(_connectionString);

        protected async Task AssertSubscriptionRules(Type[] messageTypes, string subscription, 
            string messagePropertyName = "MessageType")
        {
            var asyncRules = _serviceBusAdminClient.GetRulesAsync(_topic, subscription);

            var rules = new List<RuleProperties>();
            await foreach (var rule in asyncRules)
            {
                rules.Add(rule);
            }

            Assert.Equal(messageTypes.Length, rules.Count);
            foreach (var messageType in messageTypes)
            {
                var filter = new CorrelationRuleFilter();
                filter.ApplicationProperties.Add(messagePropertyName, messageType.Name);
                Assert.Single(rules.Where(r => r.Filter.Equals(filter)));
                Assert.Single(rules.Where(r => r.Name == messageType.Name));
            }
        }

        protected async Task CreateSubscriptionAsync(string subscription)
        {
            var existingSubscription = await _serviceBusAdminClient.GetSubscriptionAsync(_topic, subscription);
            if (existingSubscription.Value is null)
            {
                await _serviceBusAdminClient.CreateSubscriptionAsync(_topic, subscription);
            }
        }

        protected async Task DeleteSubscriptionAsync(string subscription) 
            => await _serviceBusAdminClient.DeleteSubscriptionAsync(_topic, subscription);
    }
}