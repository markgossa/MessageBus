using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusAdminClientTests : AzureServiceBusAdminClientTestsBase
    {
        [Fact]
        public async Task AddsStandardSubscriptionRule()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                BuildSubscriptionFilter<AircraftLanded>()),
            };

            var subscription = nameof(AddsStandardSubscriptionRule);
            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings.First(), subscription);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task DoesNotThrowIfStandardSubscriptionRuleIfExists()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                BuildSubscriptionFilter<AircraftLanded>()),
            };

            var subscription = nameof(DoesNotThrowIfStandardSubscriptionRuleIfExists);
            await DeleteSubscriptionAsync(subscription);
            await CreateSubscriptionAsync(subscription);
            await CreateSubscriptionRulesAsync(messageHandlerMappings, subscription);

            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings[0], subscription);
            await DeleteSubscriptionAsync(subscription);
        }
        
        [Fact]
        public async Task UpdatesExistingIncorrectStandardSubscriptionRule()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                BuildSubscriptionFilter<AircraftLanded>()),
            };
            
            var incorrectMessageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                BuildSubscriptionFilter<AircraftTakenOff>())
            };

            var subscription = nameof(UpdatesExistingIncorrectStandardSubscriptionRule);
            await DeleteSubscriptionAsync(subscription);
            await CreateSubscriptionAsync(subscription);
            await CreateSubscriptionRulesAsync(incorrectMessageHandlerMappings, subscription);

            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings[0], subscription);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task AddsCustomSubscriptionRule()
        {
            var customMessageProperties = new Dictionary<string, string>
            {
                { "AircraftType", "Commercial" },
                { "AircraftSize", "Heavy" }
            };

            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                    BuildSubscriptionFilter<AircraftLanded>(customMessageProperties))
            };

            var subscription = nameof(AddsCustomSubscriptionRule);
            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings.First(), subscription);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task DoesNotThrowIfCustomSubscriptionRuleIfExists()
        {
            var customMessageProperties = new Dictionary<string, string>
            {
                { "EventType", "Urgent" },
                { "Importance", "Very" }
            };

            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                BuildSubscriptionFilter<AircraftLanded>(customMessageProperties)),
            };

            var subscription = nameof(DoesNotThrowIfCustomSubscriptionRuleIfExists);
            await DeleteSubscriptionAsync(subscription);
            await CreateSubscriptionAsync(subscription);
            await CreateSubscriptionRulesAsync(messageHandlerMappings, subscription);

            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings[0], subscription);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task UpdatesExistingIncorrectCustomSubscriptionRule()
        {
            var correctCustomMessageProperties = new Dictionary<string, string>
            {
                { "EventType", "Urgent" },
                { "Importance", "Very" }
            };
            
            var correctMessageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                BuildSubscriptionFilter<AircraftLanded>(correctCustomMessageProperties)),
            };
            
            var incorrectCustomMessageProperties = new Dictionary<string, string>
            {
                { "Importance", "NotVery" }
            };

            var incorrectMessageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                BuildSubscriptionFilter<AircraftLanded>(incorrectCustomMessageProperties)),
            };

            var subscription = nameof(UpdatesExistingIncorrectCustomSubscriptionRule);
            await DeleteSubscriptionAsync(subscription);
            await CreateSubscriptionAsync(subscription);
            await CreateSubscriptionRulesAsync(incorrectMessageHandlerMappings, subscription);

            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription)
                .ConfigureAsync(correctMessageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(correctMessageHandlerMappings[0], subscription);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task AddsMultipleCustomSubscriptionRules()
        {
            var customMessageProperties1 = new Dictionary<string, string>
            {
                { "AircraftType", "Commercial" },
                { "AircraftSize", "Heavy" }
            };

            var customMessageProperties2 = new Dictionary<string, string>
            {
                { "TakeOffSpeed", "130kts" }
            };

            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                    BuildSubscriptionFilter<AircraftLanded>(customMessageProperties1)),
                new MessageHandlerMapping(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler),
                    BuildSubscriptionFilter<AircraftLanded>(customMessageProperties2))
            };

            var subscription = nameof(AddsMultipleCustomSubscriptionRules);
            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings[0], subscription);
            await AssertSubscriptionRules(messageHandlerMappings[1], subscription);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task AddsMultipleSubscriptionRules()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                    BuildSubscriptionFilter<AircraftLanded>()),
                new MessageHandlerMapping(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler),
                    BuildSubscriptionFilter<AircraftTakenOff>())
            };

            var subscription = nameof(AddsMultipleSubscriptionRules);
            await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings[0], subscription);
            await AssertSubscriptionRules(messageHandlerMappings[1], subscription);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task HealthCheckReturnsFalseIfInvalidTopic()
        {
            var subscription = nameof(HealthCheckReturnsFalseIfInvalidTopic);
            await CreateSubscriptionAsync(subscription);
            var isHealthy = await new AzureServiceBusAdminClient(_hostname, "invalidTopic", subscription, _tenantId)
                .CheckHealthAsync();

            Assert.False(isHealthy);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task HealthCheckReturnsTrueIfValidSettings()
        {
            var subscription = nameof(HealthCheckReturnsTrueIfValidSettings);
            await CreateSubscriptionAsync(subscription);
            var isHealthy = await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId)
                .CheckHealthAsync();

            Assert.True(isHealthy);
            await DeleteSubscriptionAsync(subscription);
        }

        [Fact]
        public async Task HealthCheckReturnsFalseIfInvalidSubscription()
        {
            var isHealthy = await new AzureServiceBusAdminClient(_hostname, _topic, "invalidSubscription", _tenantId)
                .CheckHealthAsync();

            Assert.False(isHealthy);
        }

        [Fact]
        public async Task CreatesSubscriptionWithCustomOptionsMI()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                BuildSubscriptionFilter<AircraftLanded>()),
            };

            var subscription = nameof(CreatesSubscriptionWithCustomOptionsMI);
            await DeleteSubscriptionAsync(subscription);
            var createSubscriptionOptions = new CreateSubscriptionOptions(_topic, subscription)
            {
                LockDuration = TimeSpan.FromSeconds(60),
                MaxDeliveryCount = 5,
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(300)
            };
            await new AzureServiceBusAdminClient(_hostname, _tenantId, createSubscriptionOptions)
                .ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings[0], subscription);
            await AssertSubscriptionOptions(subscription, createSubscriptionOptions);
        }

        [Fact]
        public async Task CreatesSubscriptionWithCustomOptionsConnStr()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                    BuildSubscriptionFilter<AircraftLanded>()),
            };

            var subscription = nameof(CreatesSubscriptionWithCustomOptionsConnStr);
            await DeleteSubscriptionAsync(subscription);
            var createSubscriptionOptions = new CreateSubscriptionOptions(_topic, subscription)
            {
                LockDuration = TimeSpan.FromSeconds(60),
                MaxDeliveryCount = 5,
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(300)
            };
            await new AzureServiceBusAdminClient(_connectionString, createSubscriptionOptions)
                .ConfigureAsync(messageHandlerMappings,
                new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings[0], subscription);
            await AssertSubscriptionOptions(subscription, createSubscriptionOptions);
        }

        [Fact]
        public async Task UpdatesSubscriptionCustomOptions()
        {
            var messageHandlerMappings = new List<MessageHandlerMapping>
            {
                new MessageHandlerMapping(typeof(AircraftLanded), typeof(AircraftLandedHandler),
                    BuildSubscriptionFilter<AircraftLanded>()),
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

            await new AzureServiceBusAdminClient(_hostname, _tenantId, initialSubscriptionOptions)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());
            await AssertSubscriptionOptions(subscription, initialSubscriptionOptions);
            await new AzureServiceBusAdminClient(_hostname, _tenantId, newSubscriptionOptions)
                .ConfigureAsync(messageHandlerMappings, new MessageBusOptions());

            await AssertSubscriptionRules(messageHandlerMappings[0], subscription);
            await AssertSubscriptionOptions(subscription, newSubscriptionOptions);
        }
    }
}
