using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Collections.Generic;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class SubscriptionFilterTests
    {
        private const string _defaultMessageTypePropertyName = "MessageType";

        [Theory]
        [InlineData(typeof(AircraftTakenOff))]
        [InlineData(typeof(AircraftLanded))]
        public void LabelReturnsLabelIfSet(Type typeOfMessage)
        {
            var sut = new SubscriptionFilter
            {
                Label = typeOfMessage.Name
            };
            
            sut.Build(_defaultMessageTypePropertyName, typeOfMessage);

            Assert.Equal(typeOfMessage.Name, sut.Label);
        }

        [Theory]
        [InlineData(typeof(AircraftTakenOff))]
        [InlineData(typeof(AircraftLanded))]
        public void ReturnsMessageTypePropertyIfMessageTypePropertyIsSetAndLabelIsNotSet(Type typeOfMessage)
        {
            var sut = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { _defaultMessageTypePropertyName, typeOfMessage.Name }
                }
            };

            sut.Build(_defaultMessageTypePropertyName, typeOfMessage);

            Assert.Equal(typeOfMessage.Name, sut.MessageProperties[_defaultMessageTypePropertyName]);
            Assert.Null(sut.Label);
        }
        
        [Theory]
        [InlineData(typeof(AircraftTakenOff), "MessageType2")]
        [InlineData(typeof(AircraftLanded), "MyMessageIdentifier")]
        public void ReturnsMessageTypePropertyIfCustomMessageTypePropertyIsSetAndLabelIsNotSet(Type typeOfMessage,
            string messageTypePropertyName)
        {
            var sut = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { messageTypePropertyName, typeOfMessage.Name }
                }
            };

            sut.Build(messageTypePropertyName, typeOfMessage);

            Assert.Equal(typeOfMessage.Name, sut.MessageProperties[messageTypePropertyName]);
            Assert.Null(sut.Label);
        }

        [Theory]
        [InlineData(null, null, typeof(AircraftTakenOff))]
        [InlineData("", "", typeof(AircraftTakenOff))]
        [InlineData(" ", " ",typeof(AircraftLanded))]
        public void LabelReturnsMessageTypeNamefBothLabelAndMessageNullOrWhitespace(string label, 
            string messageTypePropertyValue, Type typeOfMessage)
        {
            var sut = new SubscriptionFilter
            {
                Label = label,
                MessageProperties = new Dictionary<string, string>
                {
                    { _defaultMessageTypePropertyName, messageTypePropertyValue }
                }
            };

            sut.Build(_defaultMessageTypePropertyName, typeOfMessage);

            Assert.Equal(typeOfMessage.Name, sut.Label);
        }

        [Fact]
        public void ThrowsIfAttemptToGetLabelAndNotBuilt()
        {
            var sut = new SubscriptionFilter
            {
                Label = typeof(AircraftLanded).Name
            };

            Assert.Throws<InvalidOperationException>(() => sut.Label);
        }
        
        [Theory]
        [InlineData(null, null)]
        public void BuildValidatesInputsAndThrowsArgumentNullExceptionIfOneIsNullOrEmpty(string messageTypePropertyName,
            Type typeOfMessage)
        {
            var sut = new SubscriptionFilter
            {
                Label = typeof(AircraftLanded).Name
            };

            Assert.Throws<ArgumentNullException>(() => sut.Build(messageTypePropertyName, typeOfMessage));
        }
    }
}
