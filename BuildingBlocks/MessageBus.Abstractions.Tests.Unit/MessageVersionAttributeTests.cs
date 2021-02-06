using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageVersionAttributeTests
    {
        [Fact]
        public void MessageVersionAttributeTakesIntegerInConstructor()
        {
            var sut = new MessageVersionAttribute(1);

            Assert.IsAssignableFrom<Attribute>(sut);
        }

        [Fact]
        public void MessageVersionAttributeThrowsIfNegativeInteger() 
            => Assert.Throws<ArgumentOutOfRangeException>(() => new MessageVersionAttribute(-1));

        [Fact]
        public void MessageVersionAttributeAttributeUsageIsCorrect()
        {
            var sut = new MessageVersionAttribute(1);

            var attributeUsage = sut.GetType().GetCustomAttribute<AttributeUsageAttribute>();
            Assert.Equal(AttributeTargets.Class, attributeUsage.ValidOn);
            Assert.False(attributeUsage.Inherited);
            Assert.False(attributeUsage.AllowMultiple);
        }

        [Fact]
        public void AttributeCanBeUsedOnMessages()
        {
            var aircraftLandedEvent = new AircraftLanded();
            var attributes = aircraftLandedEvent.GetType().GetCustomAttributes<Attribute>(false);
            var messageVersionAttributes = attributes.Where(a => a.GetType().Name == nameof(MessageVersionAttribute));

            Assert.Single(messageVersionAttributes);
            Assert.Equal(1, ((MessageVersionAttribute)messageVersionAttributes.First()).Version);
        }
    }
}
