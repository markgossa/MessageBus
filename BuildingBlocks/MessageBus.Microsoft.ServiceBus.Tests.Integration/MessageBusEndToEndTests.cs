using Azure.Messaging.ServiceBus;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    [Collection("IntegrationTests")]
    public class MessageBusEndToEndTests : MessageBusTestsBase
    {
        [Fact]
        public async Task ReceivesAndSendsEvent()
        {
            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var inputSubscription = nameof(ReceivesAndSendsEvent);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<ISomeDependency, SomeDependency>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandler>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            await SendMessages(aircraftLeftRunwayEvent);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftLeftRunwayEvent.RunwayId);
            var collection = await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output");
            Assert.Single(collection,
                m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftReachedGate)
                && m.Body.ToObjectFromJson<AircraftReachedGate>().AirlineId == aircraftLeftRunwayEvent.RunwayId);
        }

        [Fact]
        public async Task ReceivesAndSendsEventsHighPerformance()
        {
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            var inputSubscription = nameof(ReceivesAndSendsEventsHighPerformance);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var serviceBusAdminClient = new AzureServiceBusAdminClient(Configuration["Hostname"],
                Configuration["Topic"], inputSubscription, Configuration["TenantId"]);
            var options = new ServiceBusProcessorOptions
            {
                PrefetchCount = 50,
                MaxConcurrentCalls = 50
            };
            var serviceBusClient = new AzureServiceBusClient(Configuration["Hostname"],
                    Configuration["Topic"], inputSubscription, Configuration["TenantId"], options);
            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<ISomeDependency, SomeDependency>()
                .AddMessageBus(serviceBusAdminClient, serviceBusClient)
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            var count = 50;
            await SendMessages(aircraftTakenOffEvent, count);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent.AircraftId);
            Assert.Equal(count, (await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output")).Count(m => 
                m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftLeftAirspace)
                && m.Body.ToObjectFromJson<AircraftLeftAirspace>().AircraftIdentifier == aircraftTakenOffEvent.AircraftId));
        }
    }
}
