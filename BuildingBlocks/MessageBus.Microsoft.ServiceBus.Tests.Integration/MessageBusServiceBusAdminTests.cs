using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceBusAdminTests : MessageBusServiceBusAdminTestsBase
    {
        [Fact]
        public async Task UpdatesSubscriptionRulesAsync()
        {
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler) };

            await new MessageBusServiceBusAdmin(_connectionString, _topic, _subscription).Configure(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) });
        }

        [Fact]
        public async Task UpdatesSubscriptionRulesWithCustomMessagePropertyNameAsync()
        {
            const string messagePropertyName = "MessageIdentifier";
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler) };

            await new MessageBusServiceBusAdmin(_connectionString, _topic, _subscription, messagePropertyName).Configure(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, messagePropertyName);
        }

        [Fact]
        public async Task UpdatesSubscriptionRulesWithMultipleHandlersAsync()
        {
            var messageHandlers = new List<Type> { typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler) };

            await new MessageBusServiceBusAdmin(_connectionString, _topic, _subscription).Configure(messageHandlers);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded), typeof(AircraftTakenOff) });
        }
    }
}
