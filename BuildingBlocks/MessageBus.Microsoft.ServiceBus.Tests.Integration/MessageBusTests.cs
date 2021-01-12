using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusTests : MessageBusTestsBase
    {
        [Fact]
        public async Task UpdatesSubscriptionRulesAsync()
        {
            var services = new ServiceCollection().SubscribeToMessage<AircraftLanded, AircraftLandedHandler>();

            await new MessageBus(_connectionString, _topic, _subscription).StartAsync(services);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) });
        }

        [Fact]
        public async Task UpdatesSubscriptionRulesWithCustomMessagePropertyNameAsync()
        {
            const string messagePropertyName = "MessageIdentifier";
            var services = new ServiceCollection().SubscribeToMessage<AircraftLanded, AircraftLandedHandler>();

            await new MessageBus(_connectionString, _topic, _subscription, messagePropertyName).StartAsync(services);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded) }, messagePropertyName);
        }

        [Fact]
        public async Task UpdatesSubscriptionRulesWithMultipleHandlersAsync()
        {
            var services = new ServiceCollection()
                .SubscribeToMessage<AircraftLanded, AircraftLandedHandler>()
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();

            await new MessageBus(_connectionString, _topic, _subscription).StartAsync(services);

            await AssertSubscriptionRules(new Type[] { typeof(AircraftLanded), typeof(AircraftTakenOff) });
        }
    }
}
