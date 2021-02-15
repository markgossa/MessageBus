﻿using MessageBus.Extensions.Microsoft.DependencyInjection;
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
        public async Task ReceivesAndSendsEventsHighPerformance()
        {
            var inputSubscription = nameof(ReceivesAndSendsEventsHighPerformance);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<ISomeDependency, SomeDependency>()
                .AddMessageBus(new AzureServiceBusAdminClient(Configuration["Hostname"],
                    Configuration["Topic"], inputSubscription, Configuration["TenantId"]),
                    CreateHighPerformanceClient(inputSubscription))
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            var count = 50;
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendMessages(aircraftTakenOffEvent, count);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == aircraftTakenOffEvent.AircraftId);
            Assert.Equal(count, (await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output")).Count(m =>
                m.ApplicationProperties["MessageType"].ToString() == nameof(AircraftLeftAirspace)
                && m.Body.ToObjectFromJson<AircraftLeftAirspace>().AircraftIdentifier == aircraftTakenOffEvent.AircraftId));
        }

        [Fact]
        public async Task ReceivesAndSendsCommand()
        {
            var inputSubscription = nameof(ReceivesAndSendsCommand);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<ISomeDependency, SomeDependency>()
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
        public async Task ReceivesAndSendsCommandsHighPerformance()
        {
            var inputSubscription = nameof(ReceivesAndSendsCommandsHighPerformance);
            await CreateEndToEndTestSubscriptions(inputSubscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<ISomeDependency, SomeDependency>()
                .AddMessageBus(new AzureServiceBusAdminClient(Configuration["Hostname"],
                    Configuration["Topic"], inputSubscription, Configuration["TenantId"]),
                    CreateHighPerformanceClient(inputSubscription))
                .SubscribeToMessage<CreateNewFlightPlan, CreateNewFlightPlanHandler>();
            var serviceProvider = services.BuildServiceProvider();
            await StartMessageBusHostedService(serviceProvider);

            var count = 50;
            var createNewFlightPlanCommand = new CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            await SendMessages(createNewFlightPlanCommand, count);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.DoesNotContain(await ReceiveMessagesForSubscriptionAsync(inputSubscription),
                m => m.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId == createNewFlightPlanCommand.Destination);
            Assert.Equal(count, (await ReceiveMessagesForSubscriptionAsync($"{inputSubscription}-Output")).Count(m =>
                m.ApplicationProperties["MessageType"].ToString() == nameof(StartEngines)
                && m.Body.ToObjectFromJson<StartEngines>().EngineId == createNewFlightPlanCommand.Destination));
        }

        [Fact]
        public async Task SendsCommand()
        {
            var subscription = nameof(SendsCommand);
            await CreateEndToEndTestSubscriptions(subscription);

            var services = new ServiceCollection();
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<ISomeDependency, SomeDependency>()
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
                .AddSingleton<ISomeDependency, SomeDependency>()
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
                .AddSingleton<ISomeDependency, SomeDependency>()
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
                .AddSingleton<ISomeDependency, SomeDependency>()
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
    }
}
