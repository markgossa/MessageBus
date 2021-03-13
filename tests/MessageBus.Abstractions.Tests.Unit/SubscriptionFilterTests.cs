using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Collections.Generic;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class SubscriptionFilterTests
    {
        private const string _defaultMessageTypePropertyName = "MessageType";
        private const string _defaultMessageVersionPropertyName = "MessageVersion";

        [Theory]
        [InlineData(typeof(AircraftTakenOff))]
        [InlineData(typeof(AircraftLanded))]
        public void LabelReturnsLabelIfSet(Type typeOfMessage)
        {
            var sut = new SubscriptionFilter
            {
                Label = typeOfMessage.Name
            };
            
            sut.Build(new MessageBusOptions(), typeOfMessage);

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

            sut.Build(new MessageBusOptions(), typeOfMessage);

            Assert.Equal(typeOfMessage.Name, sut.MessageProperties[_defaultMessageTypePropertyName]);
            Assert.Null(sut.Label);
            Assert.False(sut.MessageProperties.ContainsKey(_defaultMessageVersionPropertyName));
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

            sut.Build(new MessageBusOptions { MessageTypePropertyName = messageTypePropertyName }, typeOfMessage);

            Assert.Equal(typeOfMessage.Name, sut.MessageProperties[messageTypePropertyName]);
            Assert.Null(sut.Label);
            Assert.False(sut.MessageProperties.ContainsKey(_defaultMessageVersionPropertyName));
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

            sut.Build(new MessageBusOptions(), typeOfMessage);

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
        
        [Fact]
        public void BuildValidatesInputsAndThrowsArgumentNullExceptionIfOneIsNullOrEmpty()
        {
            var sut = new SubscriptionFilter
            {
                Label = typeof(AircraftLanded).Name
            };

            Assert.Throws<ArgumentNullException>(() => sut.Build(new MessageBusOptions(), null));
            Assert.Throws<ArgumentNullException>(() => sut.Build(null, typeof(AircraftLanded)));
        }

        [Fact]
        public void SetsMessagePropertiesToEmptyDictionaryIfSetToNull()
        {
            var sut = new SubscriptionFilter
            {
                MessageProperties = null
            };

            Assert.NotNull(sut.MessageProperties);
        }

        [Fact]
        public void AddsMessageVersionPropertyIfNoCustomMessageProperties()
        {
            var sut = new SubscriptionFilter();
            sut.Build(new MessageBusOptions(), typeof(Models.Events.V2.AircraftLanded));
            
            Assert.Equal(2, int.Parse(sut.MessageProperties[_defaultMessageVersionPropertyName]));
        }
        
        [Theory]
        [InlineData("MyMessageVersion")]
        [InlineData("Version")]
        public void AddsCustomMessageVersionPropertyIfNoCustomMessageProperties(string messageVersionPropertyName)
        {
            var sut = new SubscriptionFilter();
            sut.Build(new MessageBusOptions() { MessageVersionPropertyName = messageVersionPropertyName }, 
                typeof(Models.Events.V2.AircraftLanded));
            
            Assert.Equal(2, int.Parse(sut.MessageProperties[messageVersionPropertyName]));
        }
    }
}
