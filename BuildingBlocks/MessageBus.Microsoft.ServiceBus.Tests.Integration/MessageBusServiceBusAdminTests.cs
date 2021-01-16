using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceBusAdminTests : MessageBusServiceBusAdminTestsBase, IAsyncDisposable
    {
        [Fact]
        public async Task UpdatesSubscriptionRulesAsync()
        {
            var subscription = nameof(UpdatesSubscriptionRulesAsync);
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler) };
            await CreateSubscriptionAsync(subscription);

            await new MessageBusServiceBusAdmin(_connectionString, _topic, subscription).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, subscription);
        }

        [Fact]
        public async Task UpdatesRulesCustomMessageIdentifierAsync()
        {
            const string messagePropertyName = "MessageIdentifier";
            var subscription = nameof(UpdatesRulesCustomMessageIdentifierAsync);
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler) };
            await CreateSubscriptionAsync(subscription);

            await new MessageBusServiceBusAdmin(_connectionString, _topic, subscription, messagePropertyName).ConfigureAsync(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, subscription, messagePropertyName);
        }

        [Fact]
        public async Task UpdatesRulesWithMultipleHandlersAsync()
        {
            var subscription = nameof(UpdatesRulesWithMultipleHandlersAsync);
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler) };
            await CreateSubscriptionAsync(subscription);

            await new MessageBusServiceBusAdmin(_connectionString, _topic, subscription).ConfigureAsync(messageHandlers);

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
