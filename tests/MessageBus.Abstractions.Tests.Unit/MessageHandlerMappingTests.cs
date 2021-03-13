using MessageBus.Abstractions.Tests.Unit.Handlers;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Collections.Generic;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageHandlerMappingTests
    {
        [Fact]
        public void CanCreateInstanceWithCustomSubscriptionFilter()
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>()
            };
            var messageType = typeof(AircraftLanded);
            var messageHandlerType = typeof(AircraftLandedHandler);
            var sut = new MessageHandlerMapping(messageType, messageHandlerType, subscriptionFilter);

            Assert.Equal(messageType.FullName, sut.MessageType.FullName);
            Assert.Equal(messageHandlerType.FullName, sut.MessageHandlerType.FullName);
            Assert.Equal(subscriptionFilter, sut.SubscriptionFilter);
        }
        
        [Fact]
        public void ThrowsIfNullSubscriptionFilter()
        {
            var messageType = typeof(AircraftLanded);
            var messageHandlerType = typeof(AircraftLandedHandler);

            Assert.Throws<ArgumentNullException>(() => new MessageHandlerMapping(messageType, messageHandlerType, null));
        }
    }
}
