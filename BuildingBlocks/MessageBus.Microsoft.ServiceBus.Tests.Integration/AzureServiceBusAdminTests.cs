using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusAdminTests : AzureServiceBusAdminTestsBase, IAsyncDisposable
    {
        [Fact]
        public async Task UpdatesSubscriptionRulesAsync()
        {
            var subscription = nameof(UpdatesSubscriptionRulesAsync);
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler) };

            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, subscription);
        }

        [Fact]
        public async Task UpdatesRulesCustomMessageIdentifierAsync()
        {
            const string messagePropertyName = "MessageIdentifier";
            var subscription = nameof(UpdatesRulesCustomMessageIdentifierAsync);
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler) };

            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription, null, messagePropertyName)
                .ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, subscription, messagePropertyName);
        }

        [Fact]
        public async Task UpdatesRulesWithMultipleHandlersAsync()
        {
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler) };
            
            var subscription = nameof(UpdatesRulesWithMultipleHandlersAsync);
            await new AzureServiceBusAdminClient(_connectionString, _topic, subscription).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded), typeof(AircraftTakenOff) }, subscription);
        }

        public async ValueTask DisposeAsync()
        {
            await DeleteSubscriptionAsync(nameof(UpdatesSubscriptionRulesAsync));
            await DeleteSubscriptionAsync(nameof(UpdatesRulesCustomMessageIdentifierAsync));
            await DeleteSubscriptionAsync(nameof(UpdatesRulesWithMultipleHandlersAsync));
        }
    }
}
