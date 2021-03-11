using MessageBus.Abstractions;
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
    public class MessageHandlerResolverTests
    {
        [Fact]
        public void MessageHandlerResolverReturnsMessageHandlerInstanceForGivenMessageType()
        {
            var services = new ServiceCollection();
            var sut = new MessageHandlerResolver(services);
            sut.SubcribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>(nameof(AircraftTakenOff));
            sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(nameof(AircraftLanded));
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
                MessageProperties = new Dictionary<string, string>
                    {
                        { "AircraftType", "Commercial" }
                    }
            };

            var sut = new MessageHandlerResolver(new ServiceCollection());
            sut.SubcribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>(nameof(AircraftTakenOff));
            sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(nameof(AircraftLanded), subscriptionFilter);
            var messageSubscriptions = sut.GetMessageSubscriptions();

            Assert.Equal(2, messageSubscriptions.Count());
            Assert.Single(messageSubscriptions.Where(m => m.MessageHandlerType == typeof(AircraftLandedHandler)));
            Assert.Single(messageSubscriptions.Where(m => m.MessageHandlerType == typeof(AircraftTakenOffHandler)));
            Assert.Equal(subscriptionFilter.MessageProperties, messageSubscriptions.First(m => m.MessageHandlerType == typeof(AircraftLandedHandler)).CustomSubscriptionFilterProperties);
        }

        [Fact]
        public void MessageHandlerResolverReturnsMessageHandlerInstanceForCustomSubscriptionFilterProperties()
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                    {
                        { "MessageType", "AL" }
                    }
            };

            var services = new ServiceCollection();
            var sut = new MessageHandlerResolver(services);
            sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(null, subscriptionFilter);
            sut.Initialize();
            var handler = sut.Resolve("AL");

            Assert.NotNull(handler);
            Assert.IsType<AircraftLandedHandler>(handler);

            var messageContext = new MessageContext<AircraftLanded>(new BinaryData("Hello world!"), new object(),
                new Mock<IMessageBus>().Object);
            typeof(AircraftLandedHandler).GetMethod("HandleAsync").Invoke(handler, new object[] { messageContext });
        }

        [Fact]
        public void MessageHandlerResolverThrowsIfNoMessageTypeInCustomSubscriptionFilterProperties()
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                    {
                        { "SomethingElse", "AL" }
                    }
            };

            var services = new ServiceCollection();
            var sut = new MessageHandlerResolver(services);
            sut.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(null, subscriptionFilter);
            sut.Initialize();

            object testCode() => sut.Resolve("AL");

            Assert.Throws<MessageHandlerNotFoundException>(testCode);
        }
    }
}
