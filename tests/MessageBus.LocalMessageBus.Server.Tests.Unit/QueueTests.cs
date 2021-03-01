using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using MessageBus.LocalMessageBus.Server.Tests.Unit.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class QueueTests
    {
        [Fact]
        public void EnqueuesAndDequeuesMessage()
        {
            var passengerBoardedEvent = new PassengerBoarded { PassengerId = Guid.NewGuid() };
            var passengerBoardedEventAsJson = JsonSerializer.Serialize(passengerBoardedEvent);
            var message = new LocalMessage(passengerBoardedEventAsJson);

            var sut = new MessageQueue();
            sut.Enqueue(message);
            var receivedMessage = sut.Dequeue();

            Assert.Equal(passengerBoardedEventAsJson, receivedMessage.Body);
        }

        [Fact]
        public void EnqueuesAndDequeuesMultipleMessages()
        {
            var messages = new List<LocalMessage>();
            var count = 10;
            var sut = new MessageQueue();
            for (var i = 0; i < count; i++)
            {
                var passengerBoardedEvent = new PassengerBoarded { PassengerId = Guid.NewGuid() };
                var passengerBoardedEventAsJson = JsonSerializer.Serialize(passengerBoardedEvent);
                var message = new LocalMessage(passengerBoardedEventAsJson);
                messages.Add(message);
                sut.Enqueue(message);
            }

            var receivedMessages = new List<LocalMessage>();
            for (var i = 0; i < count; i++)
            {
                receivedMessages.Add(sut.Dequeue());
            }

            Assert.Equal(count, receivedMessages.Count);

            foreach (var message in messages)
            {
                Assert.Single(receivedMessages.Where(m => m.Body == message.Body));
            }
        }
    }
}
