﻿using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    [Collection("IntegrationTests")]
    public class AzureServiceBusClientTests : MessageBusTestsBase
    {
        private readonly Mock<ITestHandler> mockTestHandler = new Mock<ITestHandler>();

        [Fact]
        public async Task CallsCorrectMessageHandlerUsingConnectionString()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            var subscription = nameof(CallsCorrectMessageHandlerUsingConnectionString);
            var aircraftTakenOffEvent = await CreateSubscriptionAndSendAircraftTakenOffEvent(subscription);

            var sut = new AzureServiceBusClient(_connectionString, _topic, subscription);
            AddHandlers(mockTestHandler, sut);
            await sut.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));
            mockTestHandler.Verify(m => m.MessageHandler(It.Is<MessageReceivedEventArgs>(m =>
                GetAircraftIdFromMessage(m.Message) == aircraftTakenOffEvent.AircraftId)),
                Times.Once);
        }
        
        [Fact]
        public async Task ThrowsHandlerExceptionIfNoMessageHandlerFound()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            var subscription = nameof(CallsCorrectMessageHandlerUsingConnectionString);
            var aircraftTakenOffEvent = await CreateSubscriptionAndSendAircraftTakenOffEvent(subscription);

            var sut = new AzureServiceBusClient(_connectionString, _topic, subscription);
            sut.AddErrorMessageHandler(mockTestHandler.Object.ErrorMessageHandler); 
            await sut.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(10));
            mockTestHandler.Verify(m => m.ErrorMessageHandler(It.Is<MessageErrorReceivedEventArgs>(m =>
                m.Exception.GetType() == typeof(MessageHandlerNotFoundException))), Times.Exactly(10));
        }

        [Fact]
        public async Task CallsCorrectMessageHandlerUsingManagedIdentity()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            var subscription = nameof(CallsCorrectMessageHandlerUsingManagedIdentity);
            var aircraftlandedEvent = await CreateSubscriptionAndSendAircraftLandedEvent(subscription);

            var sut = new AzureServiceBusClient(_hostname, _topic, subscription, _tenantId);
            AddHandlers(mockTestHandler, sut);
            await sut.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));
            mockTestHandler.Verify(m => m.MessageHandler(It.Is<MessageReceivedEventArgs>(m =>
                GetAircraftIdFromMessage(m.Message) == aircraftlandedEvent.AircraftId)),
                Times.Once);
        }

        [Fact]
        public async Task DeadLettersMessageWithoutReasonAsync()
        {
            var subscription = nameof(DeadLettersMessageWithoutReasonAsync);
            var aircraftlandedEvent = await CreateSubscriptionAndSendAircraftLandedEvent(subscription);
            var aircraftLandedHandler = new AircraftLandedHandler();
            var mockMessageHandlerResolver = new Mock<IMessageHandlerResolver>();
            mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded))).Returns(aircraftLandedHandler);

            var sut = new AzureServiceBusClient(_hostname, _topic, nameof(DeadLettersMessageWithoutReasonAsync), _tenantId);
            var messageBus = new Abstractions.MessageBus(mockMessageHandlerResolver.Object, new Mock<IMessageBusAdminClient>().Object,
                sut);

            await messageBus.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(6));
            var messages = await ReceiveMessagesForSubscriptionAsync(subscription, true);

            Assert.Single(messages.Where(m => IsMatchingAircraftId<AircraftLanded>(m, aircraftlandedEvent.AircraftId)));
            Assert.Equal(1, aircraftLandedHandler.MessageCount);
        }

        [Fact]
        public async Task DeadLettersMessageWithReasonAsync()
        {
            const string deadLetterReason = "Json Serliazation issue";
            var subscription = nameof(DeadLettersMessageWithReasonAsync);
            var aircraftlandedEvent = await CreateSubscriptionAndSendAircraftLandedEvent(subscription);
            var aircraftLandedHandler = new AircraftLandedHandler(deadLetterReason);
            var mockMessageHandlerResolver = new Mock<IMessageHandlerResolver>();
            mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded))).Returns(aircraftLandedHandler);

            var sut = new AzureServiceBusClient(_hostname, _topic, nameof(DeadLettersMessageWithReasonAsync), _tenantId);
            var messageBus = new Abstractions.MessageBus(mockMessageHandlerResolver.Object, new Mock<IMessageBusAdminClient>().Object,
                sut);

            await messageBus.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var messages = await ReceiveMessagesForSubscriptionAsync(subscription, true);
            Assert.Equal(1, aircraftLandedHandler.MessageCount);
            Assert.Single(messages.Where(m => IsMatchingAircraftId<AircraftLanded>(m, aircraftlandedEvent.AircraftId)));
        }

        [Theory]
        [InlineData("LightAircraft", AuthenticationType.ConnectionString)]
        [InlineData("Commercial", AuthenticationType.ManagedIdentity)]
        public async Task PublishesEventBody(string aircraftType, AuthenticationType authenticationType)
        {
            var subscription = nameof(PublishesEventBody);
            await CreateSubscriptionAsync(subscription);
            var aircraftlandedEvent = new AircraftLanded { AircraftId = Guid.NewGuid().ToString() };
            var message = new Message<IEvent>(aircraftlandedEvent)
            {
                MessageProperties = new Dictionary<string, string>
            {
                { "AircraftType", aircraftType },
                { "AircraftSize", "Heavy" }
            }
            };

            var sut = BuildAzureServiceBusClient(authenticationType, subscription);
            await sut.PublishAsync(message);

            var matchingMessages = (await ReceiveMessagesForSubscriptionAsync(subscription))
                .Where(m => IsMatchingAircraftId<AircraftLanded>(m, aircraftlandedEvent.AircraftId));

            Assert.Single(matchingMessages);
            Assert.Equal(aircraftType, matchingMessages.First().ApplicationProperties["AircraftType"]);
            Assert.Equal("Heavy", matchingMessages.First().ApplicationProperties["AircraftSize"]);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData("Hello world!")]
        public async Task PublishesEventBodyAsString(string messageString)
        {
            var subscription = nameof(PublishesEventBodyAsString);
            await CreateSubscriptionAsync(subscription);
            var aircraftId = Guid.NewGuid().ToString();
            var message = new Message<IEvent>(messageString)
            {
                MessageProperties = new Dictionary<string, string>
            {
                { "AircraftId", aircraftId },
                { "AircraftSize", "Heavy" }
            }
            };

            var sut = BuildAzureServiceBusClient(AuthenticationType.ManagedIdentity, subscription);
            await sut.PublishAsync(message);

            var matchingMessages = (await ReceiveMessagesForSubscriptionAsync(subscription))
                .Where(m => m.Body.ToString() == messageString
                    && m.ApplicationProperties.TryGetValue("AircraftId", out var value)
                    && aircraftId == value.ToString());

            Assert.Single(matchingMessages);
            Assert.Equal(aircraftId, matchingMessages.First().ApplicationProperties["AircraftId"]);
            Assert.Equal("Heavy", matchingMessages.First().ApplicationProperties["AircraftSize"]);
        }

        [Theory]
        [InlineData("LightAircraft", AuthenticationType.ConnectionString)]
        [InlineData("Commercial", AuthenticationType.ManagedIdentity)]
        public async Task SendsCommandBody(string aircraftType, AuthenticationType authenticationType)
        {
            var subscription = nameof(SendsCommandBody);
            await CreateSubscriptionAsync(subscription);
            var createNewFlightPlanCommand = new CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var message = new Message<ICommand>(createNewFlightPlanCommand)
            {
                MessageProperties = new Dictionary<string, string>
            {
                { "AircraftType", aircraftType },
                { "AircraftSize", "Heavy" }
            }
            };

            var sut = BuildAzureServiceBusClient(authenticationType, subscription);
            await sut.SendAsync(message);

            var matchingMessages = (await ReceiveMessagesForSubscriptionAsync(subscription))
                .Where(m => m.Body.ToObjectFromJson<CreateNewFlightPlan>().Destination == 
                    createNewFlightPlanCommand.Destination);

            Assert.Single(matchingMessages);
            Assert.Equal(aircraftType, matchingMessages.First().ApplicationProperties["AircraftType"]);
            Assert.Equal("Heavy", matchingMessages.First().ApplicationProperties["AircraftSize"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello world!")]
        public async Task SendsCommandBodyAsString(string messageString)
        {
            var subscription = nameof(SendsCommandBodyAsString);
            await CreateSubscriptionAsync(subscription);
            var aircraftId = Guid.NewGuid().ToString();
            var message = new Message<ICommand>(messageString)
            {
                MessageProperties = new Dictionary<string, string>
            {
                { "AircraftId", aircraftId },
                { "AircraftSize", "Heavy" }
            }
            };

            var sut = BuildAzureServiceBusClient(AuthenticationType.ManagedIdentity, subscription);
            await sut.SendAsync(message);

            var matchingMessages = (await ReceiveMessagesForSubscriptionAsync(subscription))
                .Where(m => m.Body.ToString() == messageString
                    && m.ApplicationProperties.TryGetValue("AircraftId", out var value)
                    && aircraftId == value.ToString());

            Assert.Single(matchingMessages);
            Assert.Equal(aircraftId, matchingMessages.First().ApplicationProperties["AircraftId"]);
            Assert.Equal("Heavy", matchingMessages.First().ApplicationProperties["AircraftSize"]);
        }
    }
}