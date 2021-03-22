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
        [InlineData(typeof(AircraftTakenOff), "ATO")]
        [InlineData(typeof(AircraftLanded), "AL")]
        public void LabelReturnsLabelIfSet(Type typeOfMessage, string label)
        {
            var sut = new SubscriptionFilter
            {
                Label = label
            };
            
            sut.Build(new MessageBusOptions(), typeOfMessage);

            Assert.Equal(label, sut.Label);
            Assert.Equal(label, sut.EffectiveMessageLabel);
        }

        [Theory]
        [InlineData(typeof(AircraftTakenOff))]
        [InlineData(typeof(AircraftLanded))]
        public void EffectiveMessageLabelReturnsMessageTypePropertyIfMessageTypePropertyIsSetAndLabelIsNotSet(Type typeOfMessage)
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
            Assert.Equal(typeOfMessage.Name, sut.EffectiveMessageLabel);
        }

        [Theory]
        [InlineData(typeof(AircraftTakenOff), "MessageType2")]
        [InlineData(typeof(AircraftLanded), "MyMessageIdentifier")]
        public void EffectiveMessageLabelReturnsMessageTypePropertyIfCustomMessageTypePropertyIsSetAndLabelIsNotSet(Type typeOfMessage,
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
            Assert.Equal(typeOfMessage.Name, sut.EffectiveMessageLabel);
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

        [Fact]
        public void ThrowsIfNoMessageTypeInCustomSubscriptionFilterProperties()
        {
            var sut = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                    {
                        { "SomethingElse", "AL" }
                    }
            };

            sut.Build(new MessageBusOptions(), typeof(AircraftLanded));

            Assert.Throws<ArgumentNullException>(() => sut.EffectiveMessageLabel);
        }
    }
}
