using MessageBus.Abstractions.Tests.Unit.Handlers;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System.Collections.Generic;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageSubscriptionTests
    {
        [Fact]
        public void CanCreateInstanceWithCustomMessageProperties()
        {
            var messageProperties = new Dictionary<string, string>();
            var messageType = typeof(AircraftLanded);
            var messageHandlerType = typeof(AircraftLandedHandler);
            var sut = new MessageSubscription(messageType, messageHandlerType, messageProperties);

            Assert.Equal(messageType.FullName, sut.MessageType.FullName);
            Assert.Equal(messageHandlerType.FullName, sut.MessageHandlerType.FullName);
            Assert.Equal(messageProperties, sut.CustomSubscriptionFilterProperties);
        }
        
        [Fact]
        public void CanCreateInstanceWithoutCustomMessageProperties()
        {
            var messageType = typeof(AircraftLanded);
            var messageHandlerType = typeof(AircraftLandedHandler);
            var sut = new MessageSubscription(messageType, messageHandlerType);

            Assert.Equal(messageType.FullName, sut.MessageType.FullName);
            Assert.Equal(messageHandlerType.FullName, sut.MessageHandlerType.FullName);
            Assert.Empty(sut.CustomSubscriptionFilterProperties);
        }
    }
}
