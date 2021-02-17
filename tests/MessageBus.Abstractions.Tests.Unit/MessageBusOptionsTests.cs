using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageBusOptionsTests
    {
        [Theory]
        [InlineData("MyMessageType")]
        [InlineData(null)]
        public void SetsMessageTypePropertyName(string messageTypePropertyName)
        {
            var sut = new MessageBusOptions();

            if (!string.IsNullOrWhiteSpace(messageTypePropertyName))
            {
                sut.MessageTypePropertyName = messageTypePropertyName;
                Assert.Equal(messageTypePropertyName, sut.MessageTypePropertyName);
            }
            else
            {
                Assert.Equal("MessageType", sut.MessageTypePropertyName);
            }
        }
        
        [Theory]
        [InlineData("MyMessageVersion")]
        [InlineData(null)]
        public void SetsMessageVersionPropertyName(string messageVersionPropertyName)
        {
            var sut = new MessageBusOptions();

            if (!string.IsNullOrWhiteSpace(messageVersionPropertyName))
            {
                sut.MessageVersionPropertyName= messageVersionPropertyName;
                Assert.Equal(messageVersionPropertyName, sut.MessageVersionPropertyName);
            }
            else
            {
                Assert.Equal("MessageVersion", sut.MessageVersionPropertyName);
            }
        }
    }
}
