using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusEndToEndTests : MessageBusTestsBase
    {
        [Fact]
        public async Task ReceivesSingleMessageFromSubscription()
        {
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendEvent(aircraftTakenOffEvent);
            var subscription = nameof(ReceivesSingleMessageFromSubscription);
            await DeleteSubscriptionAsync(subscription);
            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<ISomeDependency, SomeDependency>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            var serviceProvider = services.BuildServiceProvider();

            await RunMessageBusHostedService(serviceProvider);

            var messagesRemainingInSubscription = await ReceiveMessagesForSubscriptionAsync(subscription);
            Assert.DoesNotContain(messagesRemainingInSubscription,
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent.AircraftId);
        }
        
        [Fact]
        public async Task ReceivesMultipleMessagesFromSubscription()
        {
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            var count = 10;
            for (var i = 0; i < count; i++)
            {
                await SendEvent(aircraftTakenOffEvent);
            }
            var subscription = nameof(ReceivesMultipleMessagesFromSubscription);
            await DeleteSubscriptionAsync(subscription);
            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            var serviceProvider = services.BuildServiceProvider();

            await RunMessageBusHostedService(serviceProvider);

            var messagesRemainingInSubscription = await ReceiveMessagesForSubscriptionAsync(subscription);
            Assert.DoesNotContain(messagesRemainingInSubscription,
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent.AircraftId);
        }
    }
}
