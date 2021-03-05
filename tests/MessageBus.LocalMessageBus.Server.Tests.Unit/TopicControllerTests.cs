using MessageBus.LocalMessageBus.Server.Controllers;
using MessageBus.LocalMessageBus.Server.MessageEntities;
using Moq;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class TopicControllerTests : TestsBase
    {
        [Fact]
        public void SendsMessages()
        {
            var message = BuildMessage();

            var mockTopic = new Mock<ITopic>();
            var sut = new TopicController(mockTopic.Object);
            sut.SendMessage(message);

            mockTopic.Verify(m => m.Send(message), Times.Once);
        }
    }
}
