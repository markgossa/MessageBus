using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageTests
    {
        [Fact]
        public void MessageIsAbstract() => Assert.True(typeof(Message).IsAbstract);
    }
}
