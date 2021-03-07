using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Processors;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
            var inputSubscription = nameof(ReceivesAndSendsEvent);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandler>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            await SendMessages(aircraftLeftRunwayEvent);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output"),
                m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftReachedGate)
                && m.Body.ToObjectFromJson<AircraftReachedGate>().AirlineId == aircraftLeftRunwayEvent.RunwayId);
        }

        [Fact]
        public async Task ReceivesAndSendsEventBasedOnCustomFilter()
        {
            var inputSubscription = nameof(ReceivesAndSendsEventBasedOnCustomFilter);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandler>(new Dictionary<string, string> { { "MessageType", "ALR" } });
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            await SendMessages(aircraftLeftRunwayEvent, 1, "ALR");
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output"),
                m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftReachedGate)
                && m.Body.ToObjectFromJson<AircraftReachedGate>().AirlineId == aircraftLeftRunwayEvent.RunwayId);
        }

        [Fact]
        public async Task ReceivesAndSendsCommand()
        {
            var inputSubscription = nameof(ReceivesAndSendsCommand);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<SetAutopilot, SetAutopilotHandler>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            var setAutopilotCommand = new SetAutopilot { AutopilotId = Guid.NewGuid().ToString() };
            await SendMessages(setAutopilotCommand);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<CreateNewFlightPlan>().Destination == setAutopilotCommand.AutopilotId);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output"),
                m => m.ApplicationProperties["MessageType"].ToString() == nameof(MonitorAutopilot)
                && m.Body.ToObjectFromJson<MonitorAutopilot>().AutopilotIdentifider == setAutopilotCommand.AutopilotId);
        }

        [Fact]
        public async Task SendsCommand()
        {
            var subscription = nameof(SendsCommand);
            await CreateEndToEndTestSubscriptions(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddSingleton<ISendingService, SendingService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]));
            var serviceProvider = services.BuildServiceProvider();
            var setAutopilotCommand = new SetAutopilot { AutopilotId = Guid.NewGuid().ToString() };
            await serviceProvider.GetRequiredService<ISendingService>().SendAsync(setAutopilotCommand);

            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{subscription}-Output"),
                m => m.ApplicationProperties["MessageType"].ToString() == nameof(SetAutopilot)
                && m.Body.ToObjectFromJson<SetAutopilot>().AutopilotId == setAutopilotCommand.AutopilotId);
        }

        [Fact]
        public async Task SendsEvent()
        {
            var subscription = nameof(SendsEvent);
            await CreateEndToEndTestSubscriptions(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddSingleton<IPublishingService, PublishingService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]));
            var serviceProvider = services.BuildServiceProvider();
            var aircraftTakenOffEvent = new AircraftTakenOff { AircraftId = Guid.NewGuid().ToString() };
            await serviceProvider.GetRequiredService<IPublishingService>().PublishAsync(aircraftTakenOffEvent);

            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{subscription}-Output"),
               m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftTakenOff)
               && m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent.AircraftId);
        }

        [Fact]
        public async Task ReceivesAndDeadLettersEvent()
        {
            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var inputSubscription = nameof(ReceivesAndDeadLettersEvent);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandlerDeadLetter>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            await SendMessages(aircraftLeftRunwayEvent);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftLeftRunway>().RunwayId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}", deadLetter: true),
                m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftLeftRunway)
                && m.DeadLetterReason == aircraftLeftRunwayEvent.RunwayId);
        }

        [Fact]
        public async Task ReceivesAndDeadLettersCommand()
        {
            var createNewFlightPlan = new CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var subscription = nameof(ReceivesAndDeadLettersCommand);
            await CreateEndToEndTestSubscriptions(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]))
                .SubscribeToMessage<CreateNewFlightPlan, CreateNewFlightPlanHandlerDeadLetter>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            await SendMessages(createNewFlightPlan);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(subscription),
                m => m.Body.ToObjectFromJson<CreateNewFlightPlan>().Destination == createNewFlightPlan.Destination);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{subscription}", deadLetter: true),
                m => m.ApplicationProperties["MessageType"].ToString() == nameof(CreateNewFlightPlan)
                && m.DeadLetterReason == createNewFlightPlan.Destination);
        }

        [Fact]
        public async Task ReceivesAndSendsMessagesHighPerformance()
        {
            var inputSubscription = nameof(ReceivesAndSendsMessagesHighPerformance);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusAdminClient(Configuration["Hostname"],
                    Configuration["Topic"], inputSubscription, Configuration["TenantId"]),
                    CreateHighPerformanceClient(inputSubscription))
                .SubscribeToMessage<CreateNewFlightPlan, CreateNewFlightPlanHandler>()
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandlerDeadLetter>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            var count = 25;
            var createNewFlightPlanCommand = new CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var aircraftTakenOffEvent1 = BuildAircraftTakenOffEvent();
            var aircraftTakenOffEvent2 = BuildAircraftTakenOffEvent();
            var aircraftLeftRunwayEvent1 = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var aircraftLeftRunwayEvent2 = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var tasks = new List<Task>
            {
                SendMessages(createNewFlightPlanCommand, count),
                SendMessages(aircraftTakenOffEvent1, count),
                SendMessages(aircraftTakenOffEvent2, count),
                SendMessages(aircraftLeftRunwayEvent1, count),
                SendMessages(aircraftLeftRunwayEvent2, count)
            };
            await Task.WhenAll(tasks);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent1.AircraftId);
            var messages = await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output");
            var deadLetterMessages = await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}", deadLetter: true);
            Assert.Equal(count, messages.Count(m => m.ApplicationProperties["MessageType"].ToString() == nameof(StartEngines)
                && m.Body.ToObjectFromJson<StartEngines>().EngineId == createNewFlightPlanCommand.Destination));
            Assert.Equal(count, messages.Count(m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftLeftAirspace)
                && m.Body.ToObjectFromJson<AircraftLeftAirspace>().AircraftIdentifier == aircraftTakenOffEvent1.AircraftId));
            Assert.Equal(count, messages.Count(m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftLeftAirspace)
                && m.Body.ToObjectFromJson<AircraftLeftAirspace>().AircraftIdentifier == aircraftTakenOffEvent2.AircraftId));
            Assert.Equal(count, deadLetterMessages.Count(m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftLeftRunway)
                && m.DeadLetterReason == aircraftLeftRunwayEvent1.RunwayId));
            Assert.Equal(count, deadLetterMessages.Count(m => m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftLeftRunway)
                && m.DeadLetterReason == aircraftLeftRunwayEvent2.RunwayId));
        }

        [Fact]
        public async Task CallsMessageProcessors()
        {
            var subscription = nameof(CallsMessageProcessors);
            await CreateEndToEndTestSubscriptions(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddSingleton<ISendingService, SendingService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]))
                    .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandler>()
                    .AddMessagePreProcessor<TestMessageProcessor>()
                    .AddMessagePostProcessor<TestMessageProcessor>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var messageId = Guid.NewGuid().ToString();
            await SendMessages(new AircraftLeftRunway(), 1, null, messageId);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(subscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Equal(2, (await ReceiveMessagesForSubscriptionAsync($"{subscription}-Output")).Count(m =>
                m.ApplicationProperties["MessageType"].ToString() == nameof(SetAutopilot)
                && m.Body.ToObjectFromJson<SetAutopilot>().AutopilotId == messageId));
        }

        [Fact]
        public async Task SendsMessageCopy()
        {
            var inputSubscription = nameof(SendsMessageCopy);
            await CreateEndToEndTestSubscriptions(inputSubscription);
            var messageType = inputSubscription;
            var messageProperties = new Dictionary<string, string>
            {
                { "MessageType", messageType }
            };

            await StartSendMessageCopyTestService<AircraftLeftRunwayHandlerWithCopy>(inputSubscription, messageProperties);

            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            await SendMessages(aircraftLeftRunwayEvent, 1, messageType);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftLeftRunway>().RunwayId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Equal(3, await FindAircraftReachedGateEventCount(inputSubscription, aircraftLeftRunwayEvent));
        }

        [Fact]
        public async Task SendsMessageCopyWithDelayInSeconds()
        {
            var inputSubscription = nameof(SendsMessageCopyWithDelayInSeconds);
            await CreateEndToEndTestSubscriptions(inputSubscription);
            var messageType = inputSubscription;
            var messageProperties = new Dictionary<string, string>
            {
                { "MessageType", messageType }
            };

            await StartSendMessageCopyTestService<AircraftLeftRunwayHandlerWithCopyAndDelayInSeconds>(inputSubscription, messageProperties);

            await AssertSendsMessageCopyWithDelay(inputSubscription, messageType);
        }
        
        [Fact]
        public async Task SendsMessageCopyWithDelayedEnqueueTime()
        {
            var inputSubscription = nameof(SendsMessageCopyWithDelayedEnqueueTime);
            await CreateEndToEndTestSubscriptions(inputSubscription);
            var messageType = inputSubscription;
            var messageProperties = new Dictionary<string, string>
            {
                { "MessageType", messageType }
            };

            await StartSendMessageCopyTestService<AircraftLeftRunwayHandlerWithCopyAndDelayedEnqueueTime>(inputSubscription, messageProperties);

            await AssertSendsMessageCopyWithDelay(inputSubscription, messageType);
        }
    }
}
