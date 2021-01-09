using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using EventBus.Extensions.Microsoft.DependencyInjection;
using Xunit;
using EventBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;

namespace EventBus.Microsoft.ServiceBus.Tests.Integration
{
    [Collection("Serialize")]
    public class EventBusTests
    {
        private const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        private const string _topic = "topic1";
        private const string _subscription = "ServiceBus1";
        private readonly SubscriptionClient _subscriptionClient = new SubscriptionClient(_connectionString, _topic, _subscription);

        [Fact]
        public async Task UpdatesSubscriptionRulesAsync()
        {
            var services = new ServiceCollection().SubscribeToMessage<CarWashed, CarWashedHandler>();

            await new EventBus(_connectionString, _topic, _subscription).InitializeAsync(services);

            await AssertSubscriptionRules(new Type[] { typeof(CarWashed) });
        }

        [Fact]
        public async Task UpdatesSubscriptionRulesWithCustomMessagePropertyNameAsync()
        {
            const string messagePropertyName = "MessageIdentifier";
            var services = new ServiceCollection().SubscribeToMessage<CarWashed, CarWashedHandler>();

            await new EventBus(_connectionString, _topic, _subscription, messagePropertyName).InitializeAsync(services);

            await AssertSubscriptionRules(new Type[] { typeof(CarWashed) }, messagePropertyName);
        }
        
        [Fact]
        public async Task UpdatesSubscriptionRulesWithMultipleHandlersAsync()
        {
            var services = new ServiceCollection()
                .SubscribeToMessage<CarWashed, CarWashedHandler>()
                .SubscribeToMessage<CarPainted, CarPaintedHandler>();

            await new EventBus(_connectionString, _topic, _subscription).InitializeAsync(services);

            await AssertSubscriptionRules(new Type[] { typeof(CarWashed), typeof(CarPainted) });
        }

        private async Task AssertSubscriptionRules(Type[] messageTypes, string messagePropertyName = "MessageType")
        {
            var rules = await _subscriptionClient.GetRulesAsync();
            Assert.Equal(messageTypes.Count(), rules.Count());
            foreach (var messageType in messageTypes)
            {
                var sqlFilter = new SqlFilter($"{messagePropertyName} = '{nameof(CarWashed)}'");
                Assert.Single(rules.Where(r => r.Filter.Equals(sqlFilter)));
                Assert.Single(rules.Where(r => r.Name == nameof(CarWashed)));
            }
        }
    }
}
