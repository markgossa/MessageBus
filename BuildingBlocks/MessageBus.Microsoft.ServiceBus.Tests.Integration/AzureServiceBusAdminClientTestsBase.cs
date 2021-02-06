using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            string messageTypePropertyName = "MessageType", string messageVersionPropertyName = "MessageVersion")
        {
            var asyncRules = _serviceBusAdminClient.GetRulesAsync(_topic, subscription);

            var actualRules = new List<RuleProperties>();
            await foreach (var rule in asyncRules)
            {
                actualRules.Add(rule);
            }

            Assert.Equal(messageTypes.Length, actualRules.Count);
            foreach (var messageType in messageTypes)
            {
                var expectedCorrelationRuleFilter = BuildCorrelationRuleFilter(messageTypePropertyName, messageVersionPropertyName, 
                    messageType);
                var actualRulesForMessageType = actualRules.Where(r => r.Name == messageType.Name);

                Assert.Single(actualRulesForMessageType);
                Assert.Equal(expectedCorrelationRuleFilter, actualRulesForMessageType.First().Filter);
            }
        }

        private static CorrelationRuleFilter BuildCorrelationRuleFilter(string messageTypePropertyName, 
            string messageVersionPropertyName, Type messageType)
        {
            var filter = new CorrelationRuleFilter();
            filter.ApplicationProperties.Add(messageTypePropertyName, messageType.Name);
            
            var messageVersion = messageType.GetCustomAttribute<MessageVersionAttribute>();
            if (messageVersion is not null)
            {
                filter.ApplicationProperties.Add(messageVersionPropertyName, messageVersion.Version);
            }
            
            return filter;
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