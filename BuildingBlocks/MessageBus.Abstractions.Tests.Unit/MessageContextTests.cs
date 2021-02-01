using MessageBus.Abstractions.Tests.Unit.Models.Events;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageContextTests : MessageContextTestsBase
    {
        [Fact]
        public void ReturnsMessageAsString() => Assert.Equal(_messageAsString, _sut.Body.ToString());

        [Fact]
        public void ReturnsDeserializedMessage()
        {
            var result = _sut.Body.ToObjectFromJson<AircraftTakenOff>();

            Assert.Equal(_aircraftId, result.AircraftId);
            Assert.Equal(_aircraftId, _sut.Message.AircraftId);
        }

        [Fact]
        public void CanCreateInstanceOfMessageContext()
        {
            var typeArg = typeof(AircraftTakenOff);
            var messageContextType = typeof(MessageContext<>).MakeGenericType(typeArg);
            var messageContext = Activator.CreateInstance(messageContextType, new object[] { new BinaryData(_messageAsString), 
                new object(), _mockMessageBusReceiver.Object });

            Assert.Equal(typeof(MessageContext<AircraftTakenOff>), messageContext.GetType());
        }

        [Fact]
        public async Task DeadLettersMessageAsync()
        {
            await _sut.DeadLetterMessageAsync();

            _mockMessageBusReceiver.Verify(m => m.DeadLetterMessageAsync(_messageObject, null), Times.Once);
        }

        [Fact]
        public async Task DeadLetterAsyncAvailableToInterface()
        {
            await (_sut as IMessageContext<AircraftLanded>).DeadLetterMessageAsync();

            _mockMessageBusReceiver.Verify(m => m.DeadLetterMessageAsync(_messageObject, null), Times.Once);
        }
        
        [Fact]
        public async Task DeadLettersMessageWithReasonAsync()
        {
            const string deadLetterMessage = "Invalid message";
            await _sut.DeadLetterMessageAsync(deadLetterMessage);

            _mockMessageBusReceiver.Verify(m => m.DeadLetterMessageAsync(_messageObject, deadLetterMessage), Times.Once);
        }
    }
}
