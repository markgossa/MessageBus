using MessageBus.Abstractions.Tests.Unit.Models.Commands;
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
                new object(), _mockMessageBus.Object });

            Assert.Equal(typeof(MessageContext<AircraftTakenOff>), messageContext.GetType());
        }

        [Fact]
        public async Task DeadLettersMessageAsync()
        {
            await _sut.DeadLetterMessageAsync();

            _mockMessageBus.Verify(m => m.DeadLetterMessageAsync(_messageObject, null), Times.Once);
        }

        [Fact]
        public async Task DeadLetterAsyncAvailableToInterface()
        {
            await (_sut as IMessageContext<AircraftLanded>).DeadLetterMessageAsync();

            _mockMessageBus.Verify(m => m.DeadLetterMessageAsync(_messageObject, null), Times.Once);
        }
        
        [Fact]
        public async Task DeadLettersMessageWithReasonAsync()
        {
            const string deadLetterMessage = "Invalid message";
            await _sut.DeadLetterMessageAsync(deadLetterMessage);

            _mockMessageBus.Verify(m => m.DeadLetterMessageAsync(_messageObject, deadLetterMessage), Times.Once);
        }

        [Theory]
        [InlineData("MyCorrelationId")]
        [InlineData("")]
        [InlineData(null)]
        public async Task PublishesEventWithSameCorrelationIdAsReceivedMessage(string expectedCorrelationId)
        {
            var aircraftLandedEvent = new AircraftLanded() { AircraftId = Guid.NewGuid().ToString() };
            var message = new Message<IEvent>(aircraftLandedEvent);
            Message<IEvent> callbackEvent = null;
            _mockMessageBus.Setup(m => m.PublishAsync(It.Is<Message<IEvent>>(e 
                => (e.Body as AircraftLanded).AircraftId == aircraftLandedEvent.AircraftId)))
                    .Callback<Message<IEvent>>(e => callbackEvent = e);

            _sut.CorrelationId = expectedCorrelationId;
            await _sut.PublishAsync(message);

            Assert.Equal(expectedCorrelationId, callbackEvent.CorrelationId);
        }
        
        [Fact]
        public async Task PublishesEventWithNewCorrelationIdIfSpecified()
        {
            var aircraftLandedEvent = new AircraftLanded() { AircraftId = Guid.NewGuid().ToString() };
            var expectedCorrelationId = Guid.NewGuid().ToString();
            var message = new Message<IEvent>(aircraftLandedEvent) { CorrelationId = expectedCorrelationId };
            Message<IEvent> callbackEvent = null;
            _mockMessageBus.Setup(m => m.PublishAsync(It.Is<Message<IEvent>>(e
                => (e.Body as AircraftLanded).AircraftId == aircraftLandedEvent.AircraftId)))
                    .Callback<Message<IEvent>>(e => callbackEvent = e);

            _sut.CorrelationId = Guid.NewGuid().ToString();
            await _sut.PublishAsync(message);

            Assert.Equal(expectedCorrelationId, callbackEvent.CorrelationId);
        }
        
        [Theory]
        [InlineData("MyCorrelationId")]
        [InlineData("")]
        [InlineData(null)]
        public async Task SendsCommandWithSameCorrelationIdAsReceivedMessage(string expectedCorrelationId)
        {
            var createNewFlightPlanCommand = new CreateNewFlightPlan() { Destination = Guid.NewGuid().ToString() };
            var message = new Message<ICommand>(createNewFlightPlanCommand);
            Message<ICommand> callbackEvent = null;
            _mockMessageBus.Setup(m => m.SendAsync(It.Is<Message<ICommand>>(e 
                => (e.Body as CreateNewFlightPlan).Destination == createNewFlightPlanCommand.Destination)))
                    .Callback<Message<ICommand>>(e => callbackEvent = e);

            _sut.CorrelationId = expectedCorrelationId;
            await _sut.SendAsync(message);

            Assert.Equal(expectedCorrelationId, callbackEvent.CorrelationId);
        }
        
        [Fact]
        public async Task SendsCommandWithNewCorrelationIdIfSpecified()
        {
            var aircraftLandedEvent = new AircraftLanded() { AircraftId = Guid.NewGuid().ToString() };
            var expectedCorrelationId = Guid.NewGuid().ToString();
            var message = new Message<IEvent>(aircraftLandedEvent) { CorrelationId = expectedCorrelationId };
            Message<IEvent> callbackEvent = null;
            _mockMessageBus.Setup(m => m.PublishAsync(It.Is<Message<IEvent>>(e
                => (e.Body as AircraftLanded).AircraftId == aircraftLandedEvent.AircraftId)))
                    .Callback<Message<IEvent>>(e => callbackEvent = e);

            _sut.CorrelationId = Guid.NewGuid().ToString();
            await _sut.PublishAsync(message);

            Assert.Equal(expectedCorrelationId, callbackEvent.CorrelationId);
        }
    }
}
