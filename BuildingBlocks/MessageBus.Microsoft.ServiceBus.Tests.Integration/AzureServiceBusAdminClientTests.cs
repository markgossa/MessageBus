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
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler) };

            var subscription = nameof(UpdatesSubscriptionRulesAsync);
            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, subscription);
            DeleteSubscriptionAsync(nameof(UpdatesSubscriptionRulesAsync)).Wait();
        }

        [Fact]
        public async Task UpdatesRulesCustomMessageIdentifierAsync()
        {
            const string messagePropertyName = "MessageIdentifier";
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler) };

            var subscription = nameof(UpdatesRulesCustomMessageIdentifierAsync);
            var options = new AzureServiceBusAdminClientOptions
            {
                MessageTypePropertyName = messagePropertyName
            };

            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription, options)
                .ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, subscription, messagePropertyName);
            DeleteSubscriptionAsync(nameof(UpdatesRulesCustomMessageIdentifierAsync)).Wait();
        }

        [Fact]
        public async Task UpdatesRulesWithMultipleHandlersAsync()
        {
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler) };

            var subscription = nameof(UpdatesRulesWithMultipleHandlersAsync);
            await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded), typeof(AircraftTakenOff) }, subscription);
            DeleteSubscriptionAsync(nameof(UpdatesRulesWithMultipleHandlersAsync)).Wait();
        }
        
        [Fact]
        public async Task UpdatesRulesWithMessageVersionDefaultPropertyAsync()
        {
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandlerV2), typeof(AircraftTakenOffHandler) };
            var subscription = nameof(UpdatesRulesWithMessageVersionDefaultPropertyAsync);

            await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(Models.V2.AircraftLanded), typeof(AircraftTakenOff) }, subscription);
            DeleteSubscriptionAsync(nameof(UpdatesRulesWithMessageVersionDefaultPropertyAsync)).Wait();
        }
        
        [Theory]
        [InlineData("Version")]
        [InlineData("MyMessageVersion")]
        public async Task UpdatesRulesWithMessageVersionCustomPropertyAsync(string messageVersionPropertyName)
        {
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandlerV2), typeof(AircraftTakenOffHandler) };
            var subscription = nameof(UpdatesRulesWithMessageVersionCustomPropertyAsync);
            var options = new AzureServiceBusAdminClientOptions
            {
                MessageVersionPropertyName = messageVersionPropertyName
            };
            
            await new AzureServiceBusAdminClient(_hostname, _topic, subscription, _tenantId, options).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(Models.V2.AircraftLanded), typeof(AircraftTakenOff) }, subscription,
                "MessageType", messageVersionPropertyName);
            DeleteSubscriptionAsync(nameof(UpdatesRulesWithMessageVersionCustomPropertyAsync)).Wait();
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
    }
}
