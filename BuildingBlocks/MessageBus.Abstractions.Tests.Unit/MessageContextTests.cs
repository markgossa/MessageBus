using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageContextTests
    {
        private const string _aircraftId = "3df0e37b-78cf-4ec5-973f-f56778e38be1";
        private readonly string _messageAsString = $"{{\"AircraftId\": \"{_aircraftId}\"}}";
        
        [Fact]
        public void ReturnsMessageAsString()
        {
            var sut = new MessageContext<AircraftLanded>
            {
                Body = new BinaryData(_messageAsString)
            };

            Assert.Equal(_messageAsString, sut.Body.ToString());
        }
        
        [Fact]
        public void ReturnsDeserializedMessage()
        {
            var sut = new MessageContext<AircraftLanded>
            {
                Body = new BinaryData(_messageAsString)
            };

            var result = sut.Body.ToObjectFromJson<AircraftLanded>();

            Assert.Equal(_aircraftId, result.AircraftId);
            Assert.Equal(_aircraftId, sut.Message.AircraftId);
        }
    }
}
