using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusAdminClientTests : AzureServiceBusAdminClientTestsBase
    {
        [Fact]
        public async Task UpdatesSubscriptionRulesAsync()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler)),
            };

            var subscription = nameof(UpdatesSubscriptionRulesAsync);
            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription).ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(typeof(AircraftLanded), subscription);
            DeleteSubscriptionAsync(nameof(UpdatesSubscriptionRulesAsync)).Wait();
        }

        [Fact]
        public async Task UpdatesRulesCustomMessageIdentifierAsync()
        {
            const string messagePropertyName = "MessageIdentifier";
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler))
            };

            var subscription = nameof(UpdatesRulesCustomMessageIdentifierAsync);
            var options = new MessageBusOptions
            {
                MessageTypePropertyName = messagePropertyName
            };

            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription)
                .ConfigureAsync(messageHandlerMappings, options);

            await AssertSubscriptionRules(typeof(AircraftLanded), subscription, messagePropertyName);
            DeleteSubscriptionAsync(nameof(UpdatesRulesCustomMessageIdentifierAsync)).Wait();
        }

        [Fact]
        public async Task UpdatesRulesWithMultipleHandlersAsync()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler)),
                new MessageHandlerMapping(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler))
            };

            var subscription = nameof(UpdatesRulesWithMultipleHandlersAsync);
            await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId).ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(typeof(AircraftLanded), subscription);
            await AssertSubscriptionRules(typeof(AircraftTakenOff), subscription);
            DeleteSubscriptionAsync(nameof(UpdatesRulesWithMultipleHandlersAsync)).Wait();
        }

        [Fact]
        public async Task UpdatesRulesWithMessageVersionDefaultPropertyAsync()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(Models.V2.AircraftLanded), typeof(AircraftLandedHandlerV2)),
                new MessageHandlerMapping(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler))
            };

            var subscription = nameof(UpdatesRulesWithMessageVersionDefaultPropertyAsync);

            await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId).ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(typeof(Models.V2.AircraftLanded), subscription);
            await AssertSubscriptionRules(typeof(AircraftTakenOff), subscription);
            DeleteSubscriptionAsync(nameof(UpdatesRulesWithMessageVersionDefaultPropertyAsync)).Wait();
        }

        [Theory]
        [InlineData("Version")]
        [InlineData("MyMessageVersion")]
        public async Task UpdatesRulesWithMessageVersionCustomPropertyAsync(string messageVersionPropertyName)
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(Models.V2.AircraftLanded), typeof(AircraftLandedHandlerV2)),
                new MessageHandlerMapping(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler))
            };

            var subscription = nameof(UpdatesRulesWithMessageVersionCustomPropertyAsync);
            var options = new MessageBusOptions
            {
                MessageVersionPropertyName = messageVersionPropertyName
            };

            await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId).ConfigureAsync(messageHandlerMappings, options);

            await AssertSubscriptionRules(typeof(Models.V2.AircraftLanded), subscription, "MessageType", messageVersionPropertyName);
            await AssertSubscriptionRules(typeof(AircraftTakenOff), subscription, "MessageType", messageVersionPropertyName);
            DeleteSubscriptionAsync(nameof(UpdatesRulesWithMessageVersionCustomPropertyAsync)).Wait();
        }

        [Fact]
        public async Task UpdatesRulesWithMessageCustomPropertyAsync()
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageTypeName", nameof(AircraftLanded) },
                    { "Version", "1" },
                    { "AircraftType", "Commercial" }
                }
            };

            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(Models.V2.AircraftLanded), typeof(AircraftLandedHandlerV2), subscriptionFilter),
                new MessageHandlerMapping(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler))
            };

            var subscription = nameof(UpdatesRulesWithMessageCustomPropertyAsync);

            await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId).ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(typeof(Models.V2.AircraftLanded), subscription, null, null, subscriptionFilter.MessageProperties);
            await AssertSubscriptionRules(typeof(AircraftTakenOff), subscription);
            DeleteSubscriptionAsync(nameof(UpdatesRulesWithMessageCustomPropertyAsync)).Wait();
        }

        [Fact]
        public async Task HealthCheckReturnsFalseIfInvalidTopic()
        {
            var subscription = nameof(HealthCheckReturnsFalseIfInvalidTopic);
            await CreateSubscriptionAsync(subscription);
            var isHealthy = await new AzureServiceBusAdminClient(_hostname, "invalidTopic", subscription, _tenantId).CheckHealthAsync();

            Assert.False(isHealthy);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task HealthCheckReturnsTrueIfValidSettings()
        {
            var subscription = nameof(HealthCheckReturnsTrueIfValidSettings);
            await CreateSubscriptionAsync(subscription);
            var isHealthy = await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId).CheckHealthAsync();

            Assert.True(isHealthy);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task HealthCheckReturnsFalseIfInvalidSubscription()
        {
            var isHealthy = await new AzureServiceBusAdminClient(_hostname, _topic, "invalidSubscription", _tenantId).CheckHealthAsync();

            Assert.False(isHealthy);
        }

        [Fact]
        public async Task CreatesSubscriptionWithCustomOptionsMI()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler)),
            };

            var subscription = nameof(CreatesSubscriptionWithCustomOptionsMI);
            await DeleteSubscriptionAsync(subscription);
            var createSubscriptionOptions = new CreateSubscriptionOptions(_topic, subscription)
            {
                LockDuration = TimeSpan.FromSeconds(60),
                MaxDeliveryCount = 5,
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(300)
            };
            await new AzureServiceBusAdminClient(_hostname, _tenantId, createSubscriptionOptions).ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(typeof(AircraftLanded), subscription);
            await AssertSubscriptionOptions(subscription, createSubscriptionOptions);
        }

        [Fact]
        public async Task CreatesSubscriptionWithCustomOptionsConnStr()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler)),
            };

            var subscription = nameof(CreatesSubscriptionWithCustomOptionsConnStr);
            await DeleteSubscriptionAsync(subscription);
            var createSubscriptionOptions = new CreateSubscriptionOptions(_topic, subscription)
            {
                LockDuration = TimeSpan.FromSeconds(60),
                MaxDeliveryCount = 5,
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(300)
            };
            await new AzureServiceBusAdminClient(_connectionString, createSubscriptionOptions).ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(typeof(AircraftLanded), subscription);
            await AssertSubscriptionOptions(subscription, createSubscriptionOptions);
        }

        [Fact]
        public async Task UpdatesSubscriptionCustomOptions()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler)),
            };
            var subscription = nameof(UpdatesSubscriptionCustomOptions);
            await DeleteSubscriptionAsync(subscription);
            var initialSubscriptionOptions = new CreateSubscriptionOptions(_topic, subscription)
            {
                LockDuration = TimeSpan.FromSeconds(60),
                MaxDeliveryCount = 5,
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(300)
            };
            var newSubscriptionOptions = new CreateSubscriptionOptions(_topic, subscription)
            {
                LockDuration = TimeSpan.FromSeconds(30),
                MaxDeliveryCount = 5,
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(150)
            };

            await new AzureServiceBusAdminClient(_hostname, _tenantId, initialSubscriptionOptions).ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());
            await AssertSubscriptionOptions(subscription, initialSubscriptionOptions);
            await new AzureServiceBusAdminClient(_hostname, _tenantId, newSubscriptionOptions).ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(typeof(AircraftLanded), subscription);
            await AssertSubscriptionOptions(subscription, newSubscriptionOptions);
        }
    }
}
