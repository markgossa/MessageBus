using MessageBus.LocalMessageBus.Server.Controllers;
using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class TopicControllerTests
    {
        [Fact]
        public void AddsSubscription()
        {
            var request = new SubscriptionRequest
            {
                Name = "Sub1",
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "ATO" }
                },
                Label = "MyLabel"
            };

            var mockTopic = new Mock<ITopic>();
            var sut = new TopicController(mockTopic.Object);
            sut.AddSubscription(request);

            mockTopic.Verify(m => m.AddSubscription(It.Is<Subscription>(s =>
                s.Label == request.Label
                && s.MessageProperties == request.MessageProperties
                && s.Name == request.Name)));
        }
    }
}
