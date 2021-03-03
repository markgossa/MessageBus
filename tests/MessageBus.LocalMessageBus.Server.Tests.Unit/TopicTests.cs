using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using MessageBus.LocalMessageBus.Server.Services;
using Moq;
using System;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class TopicTests
    {
        [Fact]
        public void CreatesANewSubscriptionAndEnqueuesMessage()
        {
            var mockQueue = new Mock<IQueue>();
            var subscription = new Subscription(mockQueue.Object, "Subscription1");
            
            var sut = new Topic();
            sut.AddSubscription(subscription);
            var messageBody = Guid.NewGuid().ToString();
            var message = new LocalMessage(messageBody);
            sut.Send(message);

            mockQueue.Verify(m => m.Send(message), Times.Once);
        }
        
        [Fact]
        public void CreatesNewSubscriptionsAndEnqueuesMessage()
        {
            var mockQueue1 = new Mock<IQueue>();
            var mockQueue2 = new Mock<IQueue>();
            var subscription1 = new Subscription(mockQueue1.Object, "Subscription1");
            var subscription2 = new Subscription(mockQueue2.Object, "Subscription2");
            
            var sut = new Topic();
            sut.AddSubscription(subscription1);
            sut.AddSubscription(subscription2);
            var messageBody = Guid.NewGuid().ToString();
            var message = new LocalMessage(messageBody);
            sut.Send(message);

            mockQueue1.Verify(m => m.Send(message), Times.Once);
            mockQueue2.Verify(m => m.Send(message), Times.Once);
        }

        [Fact]
        public void RemovesSubscriptionAndDoesNotEnqueueNewMessages()
        {
            var mockQueue1 = new Mock<IQueue>();
            var mockQueue2 = new Mock<IQueue>();
            var subscription1 = new Subscription(mockQueue1.Object, "Subscription1");
            var subscription2 = new Subscription(mockQueue2.Object, "Subscription2");

            var sut = new Topic();
            sut.AddSubscription(subscription1);
            sut.AddSubscription(subscription2);
            sut.RemoveSubscription(subscription2);
            var messageBody = Guid.NewGuid().ToString();
            var message = new LocalMessage(messageBody);
            sut.Send(message);

            mockQueue1.Verify(m => m.Send(message), Times.Once);
            mockQueue2.Verify(m => m.Send(message), Times.Never); 
        }
    }
}
