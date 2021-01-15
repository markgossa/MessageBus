using Azure.Messaging.ServiceBus;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceTestsBase
    {
        private const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
    "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        private const string _topic = "topic1";
        private const string _subscription = "ServiceBus1";
        private readonly ServiceBusClient _serviceBusClient = new ServiceBusClient(_connectionString);
        private readonly ServiceBusSender _topicClient;

        public MessageBusServiceTestsBase()
        {
            _topicClient = _serviceBusClient.CreateSender(_topic);
        }

        protected static AircraftTakenOff BuildAircraftTakenOffEvent()
            => new AircraftTakenOff
            {
                AircraftId = Guid.NewGuid().ToString(),
                FlightNumber = "BA12345",
                Timestamp = DateTime.Now
            };

        protected async Task SendMessage(AircraftTakenOff aircraftTakenOffEvent)
        {
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(aircraftTakenOffEvent)));
            message.ApplicationProperties.Add("MessageType", nameof(AircraftTakenOff));
            await _topicClient.SendMessageAsync(message);
        }
    }
}