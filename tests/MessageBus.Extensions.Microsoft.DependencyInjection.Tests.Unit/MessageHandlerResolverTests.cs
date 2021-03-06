﻿using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageHandlerResolverTests : MessageHandlerResolverTestsBase
    {
        private const string _defaultMessageTypePropertyName = "MessageType";

        [Fact]
        public void MessageHandlerResolverReturnsMessageHandlerInstanceForGivenMessageType()
        {
            var services = new ServiceCollection();
            var sut = new MessageHandlerResolver(services);
            sut.SubcribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>(BuildSubscriptionFilter<AircraftTakenOff>());
            sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(BuildSubscriptionFilter<AircraftLanded>());
            sut.Initialize();
            var handler = sut.Resolve(nameof(AircraftLanded));

            Assert.NotNull(handler);
            Assert.IsType<AircraftLandedHandler>(handler);

            var messageContext = new MessageContext<AircraftLanded>(new BinaryData("Hello world!"), new object(),
                new Mock<IMessageBus>().Object);
            typeof(AircraftLandedHandler).GetMethod("HandleAsync").Invoke(handler, new object[] { messageContext });
        }

        [Fact]
        public void MessageHandlerResolverThrowsIfCannotFindMessageHandler()
        {
            var sut = new MessageHandlerResolver(new ServiceCollection());
            Assert.Throws<MessageHandlerNotFoundException>(() => sut.Resolve("UnknownMessage"));
        }

        [Fact]
        public void AddMessageSubscriptionAddsAMessageSubscription()
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                Label = nameof(AircraftLanded),
                MessageProperties = new Dictionary<string, string>
                    {
                        { "AircraftType", "Commercial" }
                    }
            };

            subscriptionFilter.Build(new(), typeof(AircraftLanded));

            var sut = new MessageHandlerResolver(new ServiceCollection());
            sut.SubcribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>(BuildSubscriptionFilter<AircraftTakenOff>());
            sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(subscriptionFilter);
            var messageSubscriptions = sut.GetMessageHandlerMappings();

            Assert.Equal(2, messageSubscriptions.Count());
            Assert.Single(messageSubscriptions.Where(m => m.MessageHandlerType == typeof(AircraftLandedHandler)));
            Assert.Single(messageSubscriptions.Where(m => m.MessageHandlerType == typeof(AircraftTakenOffHandler)));
            Assert.Equal(subscriptionFilter.MessageProperties, messageSubscriptions.First(m => 
                m.MessageHandlerType == typeof(AircraftLandedHandler)).SubscriptionFilter.MessageProperties);
        }
        
        [Fact]
        public void AddMessageSubscriptionThrowsIfNullSubscriptionFilter()
        {
            var sut = new MessageHandlerResolver(new ServiceCollection());
            sut.SubcribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>(BuildSubscriptionFilter<AircraftTakenOff>());

            Assert.Throws<ArgumentNullException>(() => sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(null));
        }

        [Fact]
        public void MessageHandlerResolverReturnsMessageHandlerInstanceForCustomSubscriptionFilterMessageProperties()
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                    {
                        { "MessageType", "AL" }
                    }
            };

            subscriptionFilter.Build(new(), typeof(AircraftLanded));

            var services = new ServiceCollection();
            var sut = new MessageHandlerResolver(services);
            sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(subscriptionFilter);
            sut.Initialize();
            var handler = sut.Resolve("AL");

            Assert.NotNull(handler);
            Assert.IsType<AircraftLandedHandler>(handler);

            var messageContext = new MessageContext<AircraftLanded>(new BinaryData("Hello world!"), new object(),
                new Mock<IMessageBus>().Object);
            typeof(AircraftLandedHandler).GetMethod("HandleAsync").Invoke(handler, new object[] { messageContext });
        }

        [Theory]
        [InlineData("MyAircraftLanded")]
        [InlineData("ItsStillInOnePiece")]
        public void MessageHandlerResolverReturnsMessageHandlerInstanceForCustomSubscriptionFilterLabel(string label)
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                Label = label
            };
            subscriptionFilter.Build(new(), typeof(AircraftLanded));

            var services = new ServiceCollection();
            var sut = new MessageHandlerResolver(services);
            sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(subscriptionFilter);
            sut.Initialize();
            var handler = sut.Resolve(label);

            Assert.NotNull(handler);
            Assert.IsType<AircraftLandedHandler>(handler);

            var messageContext = new MessageContext<AircraftLanded>(new BinaryData("Hello world!"), new object(),
                new Mock<IMessageBus>().Object);
            typeof(AircraftLandedHandler).GetMethod("HandleAsync").Invoke(handler, new object[] { messageContext });
        }
    }
}
