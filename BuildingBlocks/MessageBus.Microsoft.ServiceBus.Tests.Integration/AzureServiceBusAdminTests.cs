using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusAdminTests : AzureServiceBusAdminTestsBase
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
            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription, null, messagePropertyName)
                .ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, subscription, messagePropertyName);
            DeleteSubscriptionAsync(nameof(UpdatesRulesCustomMessageIdentifierAsync)).Wait();
        }

        [Fact]
        public async Task UpdatesRulesWithMultipleHandlersAsync()
        {
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler) };
            
            var subscription = nameof(UpdatesRulesWithMultipleHandlersAsync);
            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded), typeof(AircraftTakenOff) }, subscription);
            DeleteSubscriptionAsync(nameof(UpdatesRulesWithMultipleHandlersAsync)).Wait();
        }
    }
}
