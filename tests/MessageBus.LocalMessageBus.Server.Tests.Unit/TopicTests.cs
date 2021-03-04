using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class TopicTests
    {
        [Fact]
        public void CreatesANewSubscriptionAndEnqueuesMessage()
        {
            var mockQueue = new Mock<IQueue>();
            var subscription = new Subscription("Subscription1", mockQueue.Object);
            
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
            var subscription1 = new Subscription("Subscription1", mockQueue1.Object);
            var subscription2 = new Subscription("Subscription2", mockQueue2.Object);

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
            var subscription1 = new Subscription("Subscription1", mockQueue1.Object);
            var subscription2 = new Subscription("Subscription2", mockQueue2.Object);

            var sut = new Topic();
            sut.AddSubscription(subscription1);
            sut.AddSubscription(subscription2);
            sut.RemoveSubscription(subscription2.Name);
            var messageBody = Guid.NewGuid().ToString();
            var message = new LocalMessage(messageBody);
            sut.Send(message);

            mockQueue1.Verify(m => m.Send(message), Times.Once);
            mockQueue2.Verify(m => m.Send(message), Times.Never); 
        }

        [Fact]
        public void UpdatesExistingSubscription()
        {
            var mockQueue = new Mock<IQueue>();
            var initialSubscription = new Subscription("Subscription1", mockQueue.Object);
            var newSubscription = new Subscription("Subscription1", mockQueue.Object)
            {
                Label = "MyLabel",
                MessageProperties = new()
                {
                    { "MessageType", "AircraftLanded"},
                    { "AircraftType", "Commercial"}
                }
            };

            var sut = new Topic();
            sut.AddSubscription(initialSubscription);
            sut.UpdateSubsription(newSubscription, "Subscription1");
            var subscriptions = sut.GetSubscriptions();

            Assert.Single(subscriptions);
            Assert.Single(subscriptions.Where(s => s.Label == newSubscription.Label));
            Assert.Single(subscriptions.Where(s => s.MessageProperties == newSubscription.MessageProperties));
        }
        
        [Fact]
        public void ThrowsIfRemoveUnknownSubscription()
        {
            var sut = new Topic();

            Assert.Throws<InvalidOperationException>(() => sut.RemoveSubscription("InvalidSubscription"));
        }
        
        [Fact]
        public void ThrowsIfUpdateUnknownSubscription()
        {
            var sut = new Topic();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var subscription = new Subscription("InvalidSubscripton", new Queue());
                sut.UpdateSubsription(subscription, "InvalidSubscripton");
            });
        }
    }
}
