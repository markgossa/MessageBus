using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusTestsBase : IDisposable, IAsyncDisposable
    {
        protected readonly IConfiguration Configuration = new Settings().Configuration;
        protected readonly string _tenantId;
        protected readonly string _hostname;
        protected readonly string _connectionString;
        protected readonly string _topic;
        protected readonly string _subscription = nameof(MessageBusTestsBase);
        protected readonly ServiceBusClient _serviceBusClient;
        protected readonly ServiceBusAdministrationClient _serviceBusAdminClient;
        protected ServiceProvider _serviceProvider;
        protected AzureServiceBusClient _azureServiceBusClient;
        protected readonly Mock<ITestHandler> mockTestHandler = new Mock<ITestHandler>();
        private readonly ServiceBusSender _serviceBusSender;

        public MessageBusTestsBase()
        {
            var options = new ServiceBusClientOptions
            {
                RetryOptions = new()
                {
                    MaxRetries = 10,
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxDelay = TimeSpan.FromSeconds(10)
                }
            };
            _serviceBusClient = new ServiceBusClient(Configuration["ConnectionString"], options);
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

        protected async Task SendMessages(IMessage message, int count = 1, string messageType = null,
            string messageId = null)
        {
            var messages = new List<ServiceBusMessage>();
            for (var i = 0; i < count; i++)
            {
                var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, message.GetType())));
                serviceBusMessage.ApplicationProperties.Add("MessageType", messageType ?? message.GetType().Name);
                if (messageId is not null)
                {
                    serviceBusMessage.MessageId = messageId;
                }

                messages.Add(serviceBusMessage);
            }

            await _serviceBusSender.SendMessagesAsync(messages);
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
            await SendMessages(aircraftTakenOffEvent);

            return aircraftTakenOffEvent;
        }
        
        protected async Task<AircraftLanded> CreateSubscriptionAndSendAircraftLandedEvent(string subscriptionName)
        {
            await CreateSubscriptionAsync(subscriptionName);
            var aircraftTakenOffEvent = new AircraftLanded { AircraftId = Guid.NewGuid().ToString() };
            await SendMessages(aircraftTakenOffEvent);

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

        protected static async Task StartMessageBusHostedService(ServiceProvider serviceProvider)
        {
            var sut = serviceProvider.GetService<IHostedService>() as MessageBusHostedService;

            await sut.StartAsync(new CancellationToken());
        }

        protected async Task CreateEndToEndTestSubscriptions(string subscription)
        {
            await DeleteSubscriptionAsync(subscription);
            await DeleteSubscriptionAsync($"{subscription}-Output");
            await CreateSubscriptionAsync($"{subscription}-Output");
        }

        protected AzureServiceBusClient CreateHighPerformanceClient(string inputSubscription)
        {
            var options = new ServiceBusProcessorOptions
            {
                PrefetchCount = 50,
                MaxConcurrentCalls = 50
            };
            var serviceBusClient = new AzureServiceBusClient(Configuration["Hostname"],
                    Configuration["Topic"], inputSubscription, Configuration["TenantId"], options);
            return serviceBusClient;
        }

        protected async Task AssertSendsMessageCopyWithDelay(string inputSubscription, string messageType)
        {
            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            await SendMessages(aircraftLeftRunwayEvent, 1, messageType);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftLeftRunwayEvent.RunwayId
                    && m.ApplicationProperties["MessageType"].ToString() == messageType);
            Assert.Equal(1, await FindAircraftReachedGateEventCount(inputSubscription, aircraftLeftRunwayEvent));
            await Task.Delay(TimeSpan.FromSeconds(12));
            Assert.Equal(1, await FindAircraftReachedGateEventCount(inputSubscription, aircraftLeftRunwayEvent));
        }

        protected async Task<int> FindAircraftReachedGateEventCount(string inputSubscription, AircraftLeftRunway aircraftLeftRunwayEvent)
            => (await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output")).Count(m =>
                    m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftReachedGate)
                    && m.Body.ToObjectFromJson<AircraftReachedGate>().AirlineId == aircraftLeftRunwayEvent.RunwayId);

        protected async Task<ServiceProvider> StartSendMessageCopyTestService<T>(string inputSubscription, string messageType)
            where T : IMessageHandler<AircraftLeftRunway>
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string> { { "MessageType", messageType } }
            };

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftLeftRunway, T>(subscriptionFilter);
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            return serviceProvider;
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_serviceProvider is not null)
            {
                await _serviceProvider?.GetService<IHostedService>()?.StopAsync(new CancellationToken());
            }

            if (_azureServiceBusClient is not null)
            {
                await _azureServiceBusClient.StopAsync();
                await _azureServiceBusClient.DisposeAsync().AsTask();
            }
        }

        protected async Task<IEnumerable<ServiceBusReceivedMessage>> FindSetAutopilotCommands(string subscription, SetAutopilot setAutopilotCommand)
            => (await ReceiveMessagesForSubscriptionAsync($"{subscription}-Output")).Where(
                m => m.ApplicationProperties["MessageType"].ToString() == nameof(SetAutopilot)
                    && m.Body.ToObjectFromJson<SetAutopilot>().AutopilotId == setAutopilotCommand.AutopilotId);

        protected async Task<IEnumerable<ServiceBusReceivedMessage>> FindAircraftTakenOffEvents(string subscription, 
            AircraftTakenOff aircraftTakenOffEvent) 
                => (await ReceiveMessagesForSubscriptionAsync($"{subscription}-Output")).Where(
                    m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftTakenOff)
                        && m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent.AircraftId);
    }
}
