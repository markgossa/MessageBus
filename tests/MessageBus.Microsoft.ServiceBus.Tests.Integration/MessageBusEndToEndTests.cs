using MessageBus.Abstractions;
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
            await CreateEndToEndTestSubscriptionsAsync(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandler>();
            _serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedServiceAsync(_serviceProvider);

            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            await SendMessagesAsync(aircraftLeftRunwayEvent, nameof(AircraftLeftRunway));
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output"),
                m => m.Subject == nameof(AircraftReachedGate)
                && m.Body.ToObjectFromJson<AircraftReachedGate>().AirlineId == aircraftLeftRunwayEvent.RunwayId);
        }

        [Fact]
        public async Task ReceivesAndSendsEventMessagePropertyFilter()
        {
            var inputSubscription = nameof(ReceivesAndSendsEventMessagePropertyFilter);
            await CreateEndToEndTestSubscriptionsAsync(inputSubscription);

            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string> { { "MessageType", "ALR" } }
            };

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandler>(subscriptionFilter);
            _serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedServiceAsync(_serviceProvider);

            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            await SendMessagesAsync(aircraftLeftRunwayEvent, null, 1, "ALR");
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output"),
                m => m.Subject == nameof(AircraftReachedGate)
                && m.Body.ToObjectFromJson<AircraftReachedGate>().AirlineId == aircraftLeftRunwayEvent.RunwayId);
        }

        [Fact]
        public async Task ReceivesAndSendsCommand()
        {
            var inputSubscription = nameof(ReceivesAndSendsCommand);
            await CreateEndToEndTestSubscriptionsAsync(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<SetAutopilot, SetAutopilotHandler>();
            _serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedServiceAsync(_serviceProvider);

            var setAutopilotCommand = new SetAutopilot { AutopilotId = Guid.NewGuid().ToString() };
            await SendMessagesAsync(setAutopilotCommand, nameof(SetAutopilot));
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<CreateNewFlightPlan>().Destination == setAutopilotCommand.AutopilotId);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output"),
                m => m.Subject == nameof(MonitorAutopilot)
                && m.Body.ToObjectFromJson<MonitorAutopilot>().AutopilotIdentifider == setAutopilotCommand.AutopilotId);
        }

        [Fact]
        public async Task SendsCommand()
        {
            var subscription = nameof(SendsCommand);
            await CreateEndToEndTestSubscriptionsAsync(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddSingleton<ISendingService, SendingService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]));
            _serviceProvider = services.BuildServiceProvider();
            var setAutopilotCommand = new SetAutopilot { AutopilotId = Guid.NewGuid().ToString() };
            await _serviceProvider.GetRequiredService<ISendingService>().SendAsync(setAutopilotCommand);

            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{subscription}-Output"),
                m => m.Subject == nameof(SetAutopilot)
                && m.Body.ToObjectFromJson<SetAutopilot>().AutopilotId == setAutopilotCommand.AutopilotId);
        }

        [Fact]
        public async Task PublishesEvent()
        {
            var subscription = nameof(PublishesEvent);
            await CreateEndToEndTestSubscriptionsAsync(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddSingleton<IPublishingService, PublishingService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]));
            _serviceProvider = services.BuildServiceProvider();
            var aircraftTakenOffEvent = new AircraftTakenOff { AircraftId = Guid.NewGuid().ToString() };
            await _serviceProvider.GetRequiredService<IPublishingService>().PublishAsync(aircraftTakenOffEvent);

            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{subscription}-Output"),
               m => m.Subject == nameof(AircraftTakenOff)
               && m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent.AircraftId);
        }

        [Fact]
        public async Task ReceivesAndDeadLettersEvent()
        {
            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var inputSubscription = nameof(ReceivesAndDeadLettersEvent);
            await CreateEndToEndTestSubscriptionsAsync(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], inputSubscription, Configuration["TenantId"]))
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandlerDeadLetter>();
            _serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedServiceAsync(_serviceProvider);

            await SendMessagesAsync(aircraftLeftRunwayEvent, nameof(AircraftLeftRunway));
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftLeftRunway>().RunwayId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}", deadLetter: true),
                m => m.Subject == nameof(AircraftLeftRunway)
                && m.DeadLetterReason == aircraftLeftRunwayEvent.RunwayId);
        }

        [Fact]
        public async Task ReceivesAndDeadLettersCommand()
        {
            var createNewFlightPlan = new CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var subscription = nameof(ReceivesAndDeadLettersCommand);
            await CreateEndToEndTestSubscriptionsAsync(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]))
                .SubscribeToMessage<CreateNewFlightPlan, CreateNewFlightPlanHandlerDeadLetter>();
            _serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedServiceAsync(_serviceProvider);

            await SendMessagesAsync(createNewFlightPlan, nameof(CreateNewFlightPlan));
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(subscription),
                m => m.Body.ToObjectFromJson<CreateNewFlightPlan>().Destination == createNewFlightPlan.Destination);
            Assert.Single(await ReceiveMessagesForSubscriptionAsync($"{subscription}", deadLetter: true),
                m => m.Subject == nameof(CreateNewFlightPlan)
                && m.DeadLetterReason == createNewFlightPlan.Destination);
        }

        [Fact]
        public async Task ReceivesAndSendsMessagesHighPerformance()
        {
            var inputSubscription = nameof(ReceivesAndSendsMessagesHighPerformance);
            await CreateEndToEndTestSubscriptionsAsync(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddMessageBus(new AzureServiceBusAdminClient(Configuration["Hostname"],
                    Configuration["Topic"], inputSubscription, Configuration["TenantId"]),
                    CreateHighPerformanceClient(inputSubscription))
                .SubscribeToMessage<CreateNewFlightPlan, CreateNewFlightPlanHandler>()
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandlerDeadLetter>();
            _serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedServiceAsync(_serviceProvider);

            var count = 25;
            var createNewFlightPlanCommand = new CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var aircraftTakenOffEvent1 = BuildAircraftTakenOffEvent();
            var aircraftTakenOffEvent2 = BuildAircraftTakenOffEvent();
            var aircraftLeftRunwayEvent1 = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var aircraftLeftRunwayEvent2 = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var tasks = new List<Task>
            {
                SendMessagesAsync(createNewFlightPlanCommand, nameof(CreateNewFlightPlan), count),
                SendMessagesAsync(aircraftTakenOffEvent1, nameof(AircraftTakenOff), count),
                SendMessagesAsync(aircraftTakenOffEvent2, nameof(AircraftTakenOff), count),
                SendMessagesAsync(aircraftLeftRunwayEvent1, nameof(AircraftLeftRunway), count),
                SendMessagesAsync(aircraftLeftRunwayEvent2, nameof(AircraftLeftRunway), count)
            };
            await Task.WhenAll(tasks);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent1.AircraftId);
            var messages = await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output");
            var deadLetterMessages = await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}", deadLetter: true);
            Assert.Equal(count, messages.Count(m => m.Subject == nameof(StartEngines)
                && m.Body.ToObjectFromJson<StartEngines>().EngineId == createNewFlightPlanCommand.Destination));
            Assert.Equal(count, messages.Count(m => m.Subject == nameof(AircraftLeftAirspace)
                && m.Body.ToObjectFromJson<AircraftLeftAirspace>().AircraftIdentifier == aircraftTakenOffEvent1.AircraftId));
            Assert.Equal(count, messages.Count(m => m.Subject == nameof(AircraftLeftAirspace)
                && m.Body.ToObjectFromJson<AircraftLeftAirspace>().AircraftIdentifier == aircraftTakenOffEvent2.AircraftId));
            Assert.Equal(count, deadLetterMessages.Count(m => m.Subject == nameof(AircraftLeftRunway)
                && m.DeadLetterReason == aircraftLeftRunwayEvent1.RunwayId));
            Assert.Equal(count, deadLetterMessages.Count(m => m.Subject == nameof(AircraftLeftRunway)
                && m.DeadLetterReason == aircraftLeftRunwayEvent2.RunwayId));
        }

        [Fact]
        public async Task CallsMessageProcessors()
        {
            var subscription = nameof(CallsMessageProcessors);
            await CreateEndToEndTestSubscriptionsAsync(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddSingleton<ISendingService, SendingService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]))
                    .SubscribeToMessage<AircraftLeftRunway, AircraftLeftRunwayHandler>()
                    .AddMessagePreProcessor<TestMessageProcessor>()
                    .AddMessagePostProcessor<TestMessageProcessor>();
            _serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedServiceAsync(_serviceProvider);

            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            var messageId = Guid.NewGuid().ToString();
            await SendMessagesAsync(new AircraftLeftRunway(), nameof(AircraftLeftRunway), 1, null, messageId);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(subscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftLeftRunwayEvent.RunwayId);
            Assert.Equal(2, (await ReceiveMessagesForSubscriptionAsync($"{subscription}-Output")).Count(m =>
                m.Subject == nameof(SetAutopilot)
                && m.Body.ToObjectFromJson<SetAutopilot>().AutopilotId == messageId));
        }

        [Fact]
        public async Task SendsMessageCopy()
        {
            var inputSubscription = nameof(SendsMessageCopy);
            await CreateEndToEndTestSubscriptionsAsync(inputSubscription);

            var messageType = inputSubscription;
            _serviceProvider = await StartSendMessageCopyTestServiceAsync<AircraftLeftRunwayHandlerWithCopy>(inputSubscription, messageType);

            var aircraftLeftRunwayEvent = new AircraftLeftRunway { RunwayId = Guid.NewGuid().ToString() };
            await SendMessagesAsync(aircraftLeftRunwayEvent, messageType, 1);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftLeftRunway>().RunwayId == aircraftLeftRunwayEvent.RunwayId
                    && m.Subject == messageType);
            Assert.Equal(3, await FindAircraftReachedGateEventCountAsync(inputSubscription, aircraftLeftRunwayEvent));
        }

        [Fact]
        public async Task SendsMessageCopyWithDelayInSeconds()
        {
            var inputSubscription = nameof(SendsMessageCopyWithDelayInSeconds);
            await CreateEndToEndTestSubscriptionsAsync(inputSubscription);
            
            var messageType = inputSubscription;
            _serviceProvider = await StartSendMessageCopyTestServiceAsync<AircraftLeftRunwayHandlerWithCopyAndDelayInSeconds>(inputSubscription, messageType);

            await AssertSendsMessageCopyWithDelay(inputSubscription, messageType);
        }
        
        [Fact]
        public async Task SendsMessageCopyWithDelayedEnqueueTime()
        {
            var inputSubscription = nameof(SendsMessageCopyWithDelayedEnqueueTime);
            await CreateEndToEndTestSubscriptionsAsync(inputSubscription);

            var messageType = inputSubscription;
            _serviceProvider = await StartSendMessageCopyTestServiceAsync<AircraftLeftRunwayHandlerWithCopyAndDelayedEnqueueTime>(inputSubscription, messageType);

            await AssertSendsMessageCopyWithDelay(inputSubscription, messageType);
        }

        [Fact]
        public async Task SendsCommandWithScheduledEnqueueTime()
        {
            var subscription = nameof(SendsCommandWithScheduledEnqueueTime);
            await CreateEndToEndTestSubscriptionsAsync(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddSingleton<ISendingService, SendingServiceWithDelay>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]));
            _serviceProvider = services.BuildServiceProvider();
            var setAutopilotCommand = new SetAutopilot { AutopilotId = Guid.NewGuid().ToString() };
            await _serviceProvider.GetRequiredService<ISendingService>().SendAsync(setAutopilotCommand);

            Assert.Empty(await FindSetAutopilotCommandsAsync(subscription, setAutopilotCommand));
            await Task.Delay(TimeSpan.FromSeconds(11));
            Assert.Single(await FindSetAutopilotCommandsAsync(subscription, setAutopilotCommand));
        }

        [Fact]
        public async Task PublishesEventWithScheduledEnqueueTime()
        {
            var subscription = nameof(PublishesEventWithScheduledEnqueueTime);
            await CreateEndToEndTestSubscriptionsAsync(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IMessageTracker, MessageTracker>()
                .AddSingleton<IPublishingService, PublishingServiceWithDelay>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["Hostname"],
                        Configuration["Topic"], subscription, Configuration["TenantId"]));
            _serviceProvider = services.BuildServiceProvider();
            var aircraftTakenOffEvent = new AircraftTakenOff { AircraftId = Guid.NewGuid().ToString() };
            await _serviceProvider.GetRequiredService<IPublishingService>().PublishAsync(aircraftTakenOffEvent);

            Assert.Empty(await FindAircraftTakenOffEventsAsync(subscription, aircraftTakenOffEvent));
            await Task.Delay(TimeSpan.FromSeconds(11));
            Assert.Single(await FindAircraftTakenOffEventsAsync(subscription, aircraftTakenOffEvent));
        }
    }
}