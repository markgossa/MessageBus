using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Collections.Generic;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class SubscriptionFilterTests
    {
        [Theory]
        [InlineData(nameof(AircraftTakenOff))]
        [InlineData(nameof(AircraftLanded))]
        public void LabelReturnsLabelIfSet(string label)
        {
            var sut = new SubscriptionFilter
            {
                Label = label
            };

            Assert.Equal(label, sut.Label);
        }

        [Theory]
        [InlineData(null, null, "AircraftTakenOff", typeof(AircraftTakenOff))]
        [InlineData("", "", "AircraftTakenOff", typeof(AircraftTakenOff))]
        [InlineData(" ", " ", "AircraftLanded", typeof(AircraftLanded))]
        public void LabelReturnsMessageTypeNamefBothLabelAndMessageNullOrWhitespace(string label, 
            string messageTypePropertyValue, string messageTypeName, Type messageTypeType)
        {
            var sut = new SubscriptionFilter
            {
                Label = label,
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", messageTypePropertyValue }
                }
            };
            sut.MessageTypePropertyName = "MessageType";
            sut.SubscriptionMessageType = messageTypeType;

            Assert.Equal(messageTypeName, sut.Label);
        }
    }
}
