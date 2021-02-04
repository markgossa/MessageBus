using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusAdminClientTestsBase
    {
        protected readonly IConfiguration Configuration = new Settings().Configuration;
        protected readonly string _tenantId;
        protected readonly string _hostname;
        protected readonly string _connectionString;
        protected readonly string _topic;
        protected readonly string _subscription = nameof(AzureServiceBusAdminClientTestsBase);
        protected readonly ServiceBusClient _serviceBusClient;
        protected readonly ServiceBusAdministrationClient _serviceBusAdminClient;

        public AzureServiceBusAdminClientTestsBase()
        {
            _serviceBusClient = new ServiceBusClient(Configuration["ConnectionString"]);
            _serviceBusAdminClient = new ServiceBusAdministrationClient(Configuration["ConnectionString"]);
            _tenantId = Configuration["TenantId"];
            _topic = Configuration["Topic"];
            _hostname = Configuration["Hostname"];
            _connectionString = Configuration["ConnectionString"];
        }

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
            SubscriptionProperties existingSubscription = null;

            try
            {
                existingSubscription = await _serviceBusAdminClient.GetSubscriptionAsync(_topic, subscription);
            }
            catch {}

            if (existingSubscription is null)
            {
                await _serviceBusAdminClient.CreateSubscriptionAsync(_topic, subscription);
            }
        }

        protected async Task DeleteSubscriptionAsync(string subscription) 
            => await _serviceBusAdminClient.DeleteSubscriptionAsync(_topic, subscription);
    }
}