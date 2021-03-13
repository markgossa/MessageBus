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
        private const string _defaultMessageTypePropertyName = "MessageType";

        public AzureServiceBusAdminClientTestsBase()
        {
            _serviceBusClient = new ServiceBusClient(Configuration["ConnectionString"]);
            _serviceBusAdminClient = new ServiceBusAdministrationClient(Configuration["ConnectionString"]);
            _tenantId = Configuration["TenantId"];
            _topic = Configuration["Topic"];
            _hostname = Configuration["Hostname"];
            _connectionString = Configuration["ConnectionString"];
        }

        protected async Task AssertSubscriptionRules(Type messageType, string subscription, 
            string messageTypePropertyName = "MessageType", string messageVersionPropertyName = "MessageVersion",
            Dictionary<string, string> customProperties = null)
        {
            var asyncRules = _serviceBusAdminClient.GetRulesAsync(_topic, subscription);

            var actualRules = new List<RuleProperties>();
            await foreach (var rule in asyncRules)
            {
                actualRules.Add(rule);
            }

            Assert.Single(actualRules.Where(r => r.Name == messageType.Name));
            var expectedCorrelationRuleFilter = BuildCorrelationRuleFilter(messageTypePropertyName, messageVersionPropertyName, 
                messageType, customProperties);
            var actualRulesForMessageType = actualRules.Where(r => r.Name == messageType.Name);

            Assert.Single(actualRulesForMessageType);
            Assert.Equal(expectedCorrelationRuleFilter, actualRulesForMessageType.First().Filter);
        }

        private static CorrelationRuleFilter BuildCorrelationRuleFilter(string messageTypePropertyName, string messageVersionPropertyName, Type messageType,
            Dictionary<string, string> customProperties = null)
                => customProperties is null || customProperties.Count == 0
                    ? BuildStandardCorrelationRuleFilter(messageTypePropertyName, messageVersionPropertyName, messageType)
                    : BuildCustomCorrelationRuleFilter(customProperties);

        private static CorrelationRuleFilter BuildStandardCorrelationRuleFilter(string messageTypePropertyName, 
            string messageVersionPropertyName, Type messageType)
        {
            var filter = new CorrelationRuleFilter
            {
                Subject = messageType.Name
            };

            var messageVersion = messageType.GetCustomAttribute<MessageVersionAttribute>();
            if (messageVersion is not null)
            {
                filter.ApplicationProperties.Add(messageVersionPropertyName, messageVersion.Version);
            }
            
            return filter;
        }

        private static CorrelationRuleFilter BuildCustomCorrelationRuleFilter(Dictionary<string, string> customProperties)
        {
            var filter = new CorrelationRuleFilter();
            foreach (var property in customProperties)
            {
                filter.ApplicationProperties.Add(property.Key, property.Value);
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
        {
            SubscriptionProperties existingSubscription = null;

            try
            {
                existingSubscription = await _serviceBusAdminClient.GetSubscriptionAsync(_topic, subscription);
            }
            catch { }

            if (existingSubscription is not null)
            {
                await _serviceBusAdminClient.DeleteSubscriptionAsync(_topic, subscription);
            }
        }

        protected async Task AssertSubscriptionOptions(string subscription, CreateSubscriptionOptions createSubscriptionOptions)
        {
            var subscriptionObject = await _serviceBusAdminClient.GetSubscriptionAsync(_topic, subscription);

            Assert.Equal(createSubscriptionOptions, new CreateSubscriptionOptions(subscriptionObject.Value));
        }

        protected static SubscriptionFilter BuildSubscriptionFilter<T>(Dictionary<string, string> customMessageProperties = null) where T : IMessage
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = customMessageProperties
            };

            subscriptionFilter.Build(new MessageBusOptions(), typeof(T));

            return subscriptionFilter;
        }
    }
}