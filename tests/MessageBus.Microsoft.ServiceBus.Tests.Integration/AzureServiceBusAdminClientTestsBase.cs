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

        protected async Task AssertSubscriptionRules(MessageHandlerMapping messageHandlerMapping, string subscription)
        {
            var asyncRules = _serviceBusAdminClient.GetRulesAsync(_topic, subscription);

            var actualRules = new List<RuleProperties>();
            await foreach (var rule in asyncRules)
            {
                actualRules.Add(rule);
            }

            Assert.Single(actualRules.Where(r => r.Name == messageHandlerMapping.MessageType.Name));
            var expectedCorrelationRuleFilter = BuildCorrelationRuleFilter(messageHandlerMapping.SubscriptionFilter);
            var actualRulesForMessageType = actualRules.Where(r => r.Name == messageHandlerMapping.MessageType.Name);

            Assert.Single(actualRulesForMessageType);
            Assert.Equal(expectedCorrelationRuleFilter, actualRulesForMessageType.First().Filter);
        }

        private static CorrelationRuleFilter BuildCorrelationRuleFilter(SubscriptionFilter subscriptionFilter)
        {
            var filter = new CorrelationRuleFilter
            {
                Subject = subscriptionFilter.Label
            };

            foreach (var property in subscriptionFilter.MessageProperties)
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