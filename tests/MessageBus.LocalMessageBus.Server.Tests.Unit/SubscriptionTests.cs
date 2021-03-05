using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class SubscriptionTests : TestsBase
    {
        [Fact]
        public void EnqueuesAndDequeuesMessage()
        {
            var passengerId = Guid.NewGuid();
            var passengerBoardedEventAsJson = BuildPassengerBoardedEventJson(passengerId);
            var message = new LocalMessage(passengerBoardedEventAsJson);
            var mockQueue = new Mock<IQueue>();
            mockQueue.Setup(m => m.Receive()).Returns(message);

            const string subscriptionName = "Subscription1";
            var sut = new Subscription(subscriptionName, mockQueue.Object);
            sut.Send(message);

            var receivedMessage = sut.Receive();
            Assert.Equal(passengerBoardedEventAsJson, receivedMessage.Body);
            Assert.Equal(subscriptionName, sut.Name);
        }

        [Theory]
        [InlineData("PassengerBoarded")]
        [InlineData("123")]
        [InlineData(null)]
        [InlineData("")]
        public void DoesNotEnqueueMessagesWhichDoNotMatchTheMessageLabel(string label)
        {
            var passengerId = Guid.NewGuid();
            var passengerBoardedEventAsJson = BuildPassengerBoardedEventJson(passengerId);
            var message = new LocalMessage(passengerBoardedEventAsJson)
            {
                Label = label
            };

            var mockQueue = new Mock<IQueue>();
            var sut = new Subscription("Subscription1", mockQueue.Object)
            {
                Label = "MyLabel"
            };
            sut.Send(message);

            mockQueue.Verify(m => m.Send(It.IsAny<LocalMessage>()), Times.Never);
            Assert.Null(sut.Receive());
        }

        [Theory]
        [InlineData("PassengerBoarded")]
        [InlineData("123")]
        [InlineData(null)]
        [InlineData("")]
        public void DoesNotEnqueueMessagesWhichDoNotMatchTheMessageProperties(string messageType)
        {
            var passengerId = Guid.NewGuid();
            var passengerBoardedEventAsJson = BuildPassengerBoardedEventJson(passengerId);
            var message = new LocalMessage(passengerBoardedEventAsJson)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", messageType }
                }
            };
            var mockQueue = new Mock<IQueue>();
            var sut = new Subscription("Subscription1", mockQueue.Object)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "AircraftTakenOff" }
                }
            };
            sut.Send(message);

            mockQueue.Verify(m => m.Send(It.IsAny<LocalMessage>()), Times.Never);
            Assert.Null(sut.Receive());
        }
        
        [Fact]
        public void EnqueuesMessagesIfNoMessagePropertyFilter()
        {
            var passengerId = Guid.NewGuid();
            var passengerBoardedEventAsJson = BuildPassengerBoardedEventJson(passengerId);
            var message = new LocalMessage(passengerBoardedEventAsJson)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "AircraftTakenOff" }
                }
            };
            var mockQueue = new Mock<IQueue>();

            var sut = new Subscription("Subscription1", mockQueue.Object)
            {
                MessageProperties = new Dictionary<string, string>()
            };
            sut.Send(message);

            mockQueue.Verify(m => m.Send(message), Times.Once);
            Assert.Null(sut.Receive());
        }
        
        [Fact]
        public void EnqueuesMessagesWhichMatchTheMessageProperties()
        {
            var passengerId = Guid.NewGuid();
            var passengerBoardedEventAsJson = BuildPassengerBoardedEventJson(passengerId);
            var message = new LocalMessage(passengerBoardedEventAsJson)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "AircraftTakenOff" }
                }
            };
            var mockQueue = new Mock<IQueue>();
            var sut = new Subscription("Subscription1", mockQueue.Object)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "AircraftTakenOff" }
                }
            };
            sut.Send(message);

            mockQueue.Verify(m => m.Send(message), Times.Once);
            Assert.Null(sut.Receive());
        }
        
        [Fact]
        public void EnqueuesMessagesWhichMatchTheMessagePropertiesMultiple1()
        {
            var passengerId = Guid.NewGuid();
            var passengerBoardedEventAsJson = BuildPassengerBoardedEventJson(passengerId);
            var message = new LocalMessage(passengerBoardedEventAsJson)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "AircraftTakenOff" },
                    { "MessageVersion", "1" }
                }
            };
            var mockQueue = new Mock<IQueue>();
            var sut = new Subscription("Subscription1", mockQueue.Object)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageVersion", "1" },
                    { "MessageType", "AircraftTakenOff" }
                }
            };
            sut.Send(message);

            mockQueue.Verify(m => m.Send(It.IsAny<LocalMessage>()), Times.Once);
            Assert.Null(sut.Receive());
        }
        
        [Fact]
        public void EnqueuesMessagesWhichMatchTheMessagePropertiesMultiple2()
        {
            var passengerId = Guid.NewGuid();
            var passengerBoardedEventAsJson = BuildPassengerBoardedEventJson(passengerId);
            var message = new LocalMessage(passengerBoardedEventAsJson)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "AircraftTakenOff" },
                    { "MessageVersion", "1" },
                    { "Class", "Business" }
                }
            };
            var mockQueue = new Mock<IQueue>();
            var sut = new Subscription("Subscription1", mockQueue.Object)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageVersion", "1" },
                    { "MessageType", "AircraftTakenOff" }
                }
            };
            sut.Send(message);

            mockQueue.Verify(m => m.Send(It.IsAny<LocalMessage>()), Times.Never);
            Assert.Null(sut.Receive());
        }
    }
}
