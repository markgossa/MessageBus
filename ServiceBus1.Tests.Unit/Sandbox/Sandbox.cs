using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ServiceBus1.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ServiceBus1.Tests.Unit.Sandbox
{
    public class Sandbox
    {
        private const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        private const string _topic = "topic1";
        private const string _subscription = "ServiceBus1";
        private readonly ServiceBusClient _serviceBusClient = new ServiceBusClient(_connectionString);
        private readonly ServiceBusAdministrationClient _serviceBusAdminClient = new ServiceBusAdministrationClient(_connectionString);
        private readonly ServiceBusSender _topicClient;

        public Sandbox()
        {
            _topicClient = _serviceBusClient.CreateSender(_topic);
        }

        [Fact]
        public async Task SendTestMessage()
        {
            var mockAircraftTakenOffHandler = new Mock<IHandleMessages<AircraftTakenOff>>();
            var services = new ServiceCollection()
                .SubscribeToMessage(typeof(AircraftTakenOff), mockAircraftTakenOffHandler.Object.GetType());
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendMessage(aircraftTakenOffEvent);
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
    }
}
