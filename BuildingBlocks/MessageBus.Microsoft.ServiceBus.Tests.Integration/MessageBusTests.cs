using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using Xunit;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using Moq;
using MessageBus.Abstractions;
using System.Text;
using System.Text.Json;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusTests
    {
        private const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        private const string _topic = "topic1";
        private const string _subscription = "ServiceBus1";
        private readonly SubscriptionClient _subscriptionClient = new SubscriptionClient(_connectionString, _topic, _subscription);

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

        //[Fact]
        //public async Task CallsHandleOnTheCorrectMessageHandler()
        //{
        //    var mockAircraftLandedHandler = new Mock<IHandleMessages<AircraftLanded>>();
        //    var services = new ServiceCollection()
        //        .SubscribeToMessage(typeof(AircraftLanded), mockAircraftLandedHandler.Object.GetType())
        //        .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();

        //    await new MessageBus(_connectionString, _topic, _subscription).StartAsync(services);
        //    var topicClient = new TopicClient(_connectionString, "topic1");
        //    var aircraftLandedEvent = new AircraftLanded
        //    {
        //        AircraftId = Guid.NewGuid().ToString(),
        //        FlightNumber = "BA12345",
        //        Timestamp = DateTime.Now
        //    };
        //    var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(aircraftLandedEvent)));
        //    await topicClient.SendAsync(message);

        //    mockAircraftLandedHandler.Verify(h => h.Handle(aircraftLandedEvent), Times.Once);
        //}

        private async Task AssertSubscriptionRules(Type[] messageTypes, string messagePropertyName = "MessageType")
        {
            var rules = await _subscriptionClient.GetRulesAsync();
            Assert.Equal(messageTypes.Count(), rules.Count());
            foreach (var messageType in messageTypes)
            {
                var sqlFilter = new SqlFilter($"{messagePropertyName} = '{nameof(AircraftLanded)}'");
                Assert.Single(rules.Where(r => r.Filter.Equals(sqlFilter)));
                Assert.Single(rules.Where(r => r.Name == nameof(AircraftLanded)));
            }
        }
    }
}
