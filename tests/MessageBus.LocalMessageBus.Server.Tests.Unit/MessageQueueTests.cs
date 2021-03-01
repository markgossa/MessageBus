using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using MessageBus.LocalMessageBus.Server.Tests.Unit.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class MessageQueueTests
    {
        [Fact]
        public void EnqueuesAndDequeuesMessage()
        {
            var sut = new MessageQueue();
            var passengerBoardedEvent = EnqueueMessages(sut).First();
            var receivedMessage = sut.Dequeue();

            Assert.Equal(passengerBoardedEvent.Body, receivedMessage.Body);
        }
        
        [Fact]
        public void ReturnsNullIfNoMessageToDequeue()
        {
            var sut = new MessageQueue();
            var receivedMessage = sut.Dequeue();

            Assert.Null(receivedMessage);
        }

        [Fact]
        public void EnqueuesAndDequeuesMultipleMessages()
        {
            var sut = new MessageQueue();
            var count = 10;
            var messages = EnqueueMessages(sut, count);

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
        
        [Fact]
        public async Task EnqueuesAndDequeuesMultipleMessagesMultiThreaded()
        {
            var sut = new MessageQueue();
            var count = 200;
            var threads = 20;
            var tasks = new List<Task>();
            for (var i = 0; i < threads; i++)
            {
                tasks.Add(Task.Run(() => EnqueueMessages(sut, count/threads)));
            }

            await Task.WhenAll(tasks);

            var receivedMessages = new List<LocalMessage>();
            for (var i = 0; i < count; i++)
            {
                receivedMessages.Add(sut.Dequeue());
            }

            Assert.Equal(count, receivedMessages.Count);
        }

        private static List<LocalMessage> EnqueueMessages(MessageQueue sut, int count = 1)
        {
            var messages = new List<LocalMessage>();
            for (var i = 0; i < count; i++)
            {
                var passengerBoardedEvent = new PassengerBoarded { PassengerId = Guid.NewGuid() };
                var passengerBoardedEventAsJson = JsonSerializer.Serialize(passengerBoardedEvent);
                var message = new LocalMessage(passengerBoardedEventAsJson);
                messages.Add(message);
                sut.Enqueue(message);
            }

            return messages;
        }
    }
}
