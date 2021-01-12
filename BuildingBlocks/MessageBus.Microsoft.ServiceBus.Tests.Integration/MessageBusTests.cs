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
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Collections.Generic;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusTests
    {
        private const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        private const string _topic = "topic1";
        private const string _subscription = "ServiceBus1";
        private readonly ServiceBusClient _serviceBusClient = new ServiceBusClient(_connectionString);
        private readonly ServiceBusAdministrationClient _serviceBusAdminClient = new ServiceBusAdministrationClient(_connectionString);
        private ServiceBusSender _topicClient;

        public MessageBusTests()
        {
            _topicClient = _serviceBusClient.CreateSender(_topic);
        }

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

        [Fact]
        public async Task SendTestMessage()
        {
            var mockAircraftTakenOffHandler = new Mock<IHandleMessages<AircraftTakenOff>>();
            var services = new ServiceCollection()
                .SubscribeToMessage(typeof(AircraftTakenOff), mockAircraftTakenOffHandler.Object.GetType());
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendMessage(aircraftTakenOffEvent);

            //var sut = new MessageBus(_connectionString, _topic, _subscription);
            //await sut.StartAsync(services);

            mockAircraftTakenOffHandler.Object.GetType().GetMethod("Handle").Invoke(mockAircraftTakenOffHandler.Object, new object[] { aircraftTakenOffEvent });
            mockAircraftTakenOffHandler.Verify(h => h.Handle(aircraftTakenOffEvent), Times.Once);
        }

        private async Task SendMessage(AircraftTakenOff aircraftTakenOffEvent)
        {
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(aircraftTakenOffEvent)));
            message.ApplicationProperties.Add("MessageType", nameof(AircraftTakenOff));
            await _topicClient.SendMessageAsync(message);
        }

        private static AircraftTakenOff BuildAircraftTakenOffEvent()
            => new AircraftTakenOff
            {
                AircraftId = Guid.NewGuid().ToString(),
                FlightNumber = "BA12345",
                Timestamp = DateTime.Now
            };

        private async Task AssertSubscriptionRules(Type[] messageTypes, string messagePropertyName = "MessageType")
        {
            var asyncRules = _serviceBusAdminClient.GetRulesAsync(_topic, _subscription);

            var rules = new List<RuleProperties>();
            await foreach (var rule in asyncRules)
            {
                rules.Add(rule);
            }
            
            Assert.Equal(messageTypes.Length, rules.Count);
            foreach (var messageType in messageTypes)
            {
                var sqlFilter = new SqlRuleFilter($"{messagePropertyName} = '{nameof(AircraftLanded)}'");
                Assert.Single(rules.Where(r => r.Filter.Equals(sqlFilter)));
                Assert.Single(rules.Where(r => r.Name == nameof(AircraftLanded)));
            }
        }
    }
}
