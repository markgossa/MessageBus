using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using Moq;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusReceiverTestsBase
    {
        protected const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        protected const string _hostname = "sb43719.servicebus.windows.net";
        protected const string _topic = "topic1";
        protected const string _tenantId = "7d4a98d2-9ed7-41f7-abd3-0884effe0ad4";
        protected readonly string _subscription = nameof(MessageBusReceiverTestsBase);
        private readonly ServiceBusClient _serviceBusClient = new ServiceBusClient(_connectionString);
        protected readonly ServiceBusAdministrationClient _serviceBusAdminClient = new ServiceBusAdministrationClient(_connectionString);
        private readonly ServiceBusSender _serviceBusSender;

        public MessageBusReceiverTestsBase()
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

        protected async Task SendEvent(IMessage message)
        {
            var messageBody = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, message.GetType())));
            messageBody.ApplicationProperties.Add("MessageType", message.GetType().Name);
            await _serviceBusSender.SendMessageAsync(messageBody);
        }

        protected async Task CreateSubscriptionAsync(string subscription)
        {
            try
            {
                await _serviceBusAdminClient.DeleteSubscriptionAsync(_topic, subscription);
            }
            catch { }
            
            await _serviceBusAdminClient.CreateSubscriptionAsync(_topic, subscription);
        }

        protected async Task DeleteSubscriptionAsync(string subscription)
            => await _serviceBusAdminClient.DeleteSubscriptionAsync(_topic, subscription);

        protected static string GetAircraftIdFromMessage(BinaryData message)
        {
            var contents = Encoding.UTF8.GetString(message);
            return JsonSerializer.Deserialize<AircraftTakenOff>(contents).AircraftId;
        }

        protected async Task<AircraftTakenOff> CreateSubscriptionAndSendAircraftTakenOffEvent(string subscriptionName)
        {
            await CreateSubscriptionAsync(subscriptionName);
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendEvent(aircraftTakenOffEvent);

            return aircraftTakenOffEvent;
        }
        
        protected async Task<AircraftLanded> CreateSubscriptionAndSendAircraftLandedEvent(string subscriptionName)
        {
            await CreateSubscriptionAsync(subscriptionName);
            var aircraftTakenOffEvent = new AircraftLanded { AircraftId = Guid.NewGuid().ToString() };
            await SendEvent(aircraftTakenOffEvent);

            return aircraftTakenOffEvent;
        }

        protected async Task SendCustomMessage(string messageText)
        {
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageText));
            await _serviceBusSender.SendMessageAsync(message);
        }

        protected static void AddHandlers(Mock<ITestHandler> mockTestHandler, AzureServiceBusClient sut)
        {
            sut.AddMessageHandler(mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(mockTestHandler.Object.ErrorMessageHandler);
        }
    }
}