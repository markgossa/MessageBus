using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceTestsBase
    {
        protected const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        protected const string _hostname = "sb43719.servicebus.windows.net";
        protected const string _topic = "topic1";
        protected const string _tenantId = "7d4a98d2-9ed7-41f7-abd3-0884effe0ad4";
        protected readonly string _subscription = nameof(MessageBusServiceTestsBase);
        private readonly ServiceBusClient _serviceBusClient = new ServiceBusClient(_connectionString);
        protected readonly ServiceBusAdministrationClient _serviceBusAdminClient = new ServiceBusAdministrationClient(_connectionString);
        private readonly ServiceBusSender _serviceBusSender;

        public MessageBusServiceTestsBase()
        {
            _serviceBusSender = _serviceBusClient.CreateSender(_topic);
            var serviceBusAdminClient = new ServiceBusAdministrationClient(_connectionString);
            serviceBusAdminClient.CreateSubscriptionAsync(new(_topic, _subscription));
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
            await _serviceBusSender.SendMessageAsync(message);
        }

        protected async Task CreateSubscriptionAsync(string subscription)
        {
            Response<SubscriptionProperties> existingSubscription = null;

            try
            {
                existingSubscription = await _serviceBusAdminClient.GetSubscriptionAsync(_topic, subscription);
            }
            catch { }
            
            if (existingSubscription?.Value is null)
            {
                await _serviceBusAdminClient.CreateSubscriptionAsync(_topic, subscription);
            }
        }

        protected async Task DeleteSubscriptionAsync(string subscription)
            => await _serviceBusAdminClient.DeleteSubscriptionAsync(_topic, subscription);
    }
}