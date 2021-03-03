using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class MessageQueueTests : TestsBase
    {
        [Fact]
        public void EnqueuesAndDequeuesMessage()
        {
            var sut = new Queue();
            var passengerBoardedEvent = EnqueueMessages(sut).First();
            var receivedMessage = sut.Receive();

            Assert.Equal(passengerBoardedEvent.Body, receivedMessage.Body);
        }

        [Fact]
        public void ReturnsNullIfNoMessageToDequeue()
        {
            var sut = new Queue();
            var receivedMessage = sut.Receive();

            Assert.Null(receivedMessage);
        }

        [Fact]
        public void EnqueuesAndDequeuesMultipleMessages()
        {
            var sut = new Queue();
            var count = 10;
            var messages = EnqueueMessages(sut, count);

            var receivedMessages = new List<LocalMessage>();
            for (var i = 0; i < count; i++)
            {
                receivedMessages.Add(sut.Receive());
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
            var sut = new Queue();
            var count = 200;
            var threads = 20;
            var tasks = new List<Task>();
            for (var i = 0; i < threads; i++)
            {
                tasks.Add(Task.Run(() => EnqueueMessages(sut, count / threads)));
            }

            await Task.WhenAll(tasks);

            var receivedMessages = new List<LocalMessage>();
            for (var i = 0; i < count; i++)
            {
                receivedMessages.Add(sut.Receive());
            }

            Assert.Equal(count, receivedMessages.Count);
        }
    }
}
