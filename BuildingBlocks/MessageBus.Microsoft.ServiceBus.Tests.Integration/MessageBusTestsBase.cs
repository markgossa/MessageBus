using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusTestsBase
    {
        protected readonly IConfiguration Configuration = new Settings().Configuration;
        protected readonly string _tenantId;
        protected readonly string _hostname;
        protected readonly string _connectionString;
        protected readonly string _topic;
        protected readonly string _subscription = nameof(MessageBusTestsBase);
        protected readonly ServiceBusClient _serviceBusClient;
        protected readonly ServiceBusAdministrationClient _serviceBusAdminClient;
        private readonly ServiceBusSender _serviceBusSender;
        
        public MessageBusTestsBase()
        {
            _serviceBusClient = new ServiceBusClient(Configuration["ConnectionString"]);
            _serviceBusAdminClient = new ServiceBusAdministrationClient(Configuration["ConnectionString"]);
            _tenantId = Configuration["TenantId"];
            _topic = Configuration["Topic"];
            _hostname = Configuration["Hostname"];
            _connectionString = Configuration["ConnectionString"];
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
        {
            try
            {
                await _serviceBusAdminClient.DeleteSubscriptionAsync(_topic, subscription);
            }
            catch { }
        }

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

        protected async Task CreateSubscriptionAndSendCustomMessage(string messageText, string subscriptionName)
        {
            await CreateSubscriptionAsync(subscriptionName); 
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageText));
            await _serviceBusSender.SendMessageAsync(message);
        }

        protected static void AddHandlers(Mock<ITestHandler> mockTestHandler, AzureServiceBusClient sut)
        {
            sut.AddMessageHandler(mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(mockTestHandler.Object.ErrorMessageHandler);
        }

        protected async Task<List<ServiceBusReceivedMessage>> ReceiveMessagesForSubscriptionAsync(string subscription,
            bool deadLetter = false)
        {
            var receiver = BuildServiceBusReceiver(subscription, deadLetter);

            var messages = new List<ServiceBusReceivedMessage>();
            ServiceBusReceivedMessage message;
            do
            {
                message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1));
                if (message is not null)
                {
                    await receiver.CompleteMessageAsync(message);
                    messages.Add(message);
                }
            } while (message is not null);

            return messages;
        }

        private ServiceBusReceiver BuildServiceBusReceiver(string subscription, bool deadLetter) 
            => deadLetter
                ? _serviceBusClient.CreateReceiver(_topic, subscription,
                    new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter })
                : _serviceBusClient.CreateReceiver(_topic, subscription);

        protected static bool IsMatchingAircraftId<T>(ServiceBusReceivedMessage message, string aircraftId)
        {
            dynamic eventObject = JsonSerializer.Deserialize<T>(message.Body.ToString());
            return eventObject.AircraftId == aircraftId;
        }

        protected AzureServiceBusClient BuildAzureServiceBusClient(AuthenticationType authenticationType, string subscription)
            => authenticationType switch
            {
                AuthenticationType.ConnectionString => new AzureServiceBusClient(_connectionString, _topic, subscription),
                AuthenticationType.ManagedIdentity => new AzureServiceBusClient(_hostname, _topic, subscription, _tenantId),
                _ => throw new NotImplementedException()
            };

        protected static async Task RunMessageBusHostedService(ServiceProvider serviceProvider, int durationInSeconds = 10)
        {
            var sut = serviceProvider.GetService<IHostedService>() as MessageBusHostedService;

            await sut.StartAsync(new CancellationToken());
            await Task.Delay(TimeSpan.FromSeconds(durationInSeconds));
        }
    }
}
