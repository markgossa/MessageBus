using MessageBus.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageBusHealthCheckTests
    {
        [Theory]
        [InlineData(true, HealthStatus.Healthy)]
        [InlineData(false, HealthStatus.Unhealthy)]
        public async Task ReturnsCorrectHealthCheckResult(bool reportedHealthStatus, 
            HealthStatus expectedHealthStatus)
        {
            var mockMessageBus = new Mock<IMessageBus>();
            mockMessageBus.Setup(m => m.CheckHealthAsync()).ReturnsAsync(reportedHealthStatus);

            var sut = new MessageBusHealthCheck(mockMessageBus.Object);
            var healthCheckResult = await sut.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(expectedHealthStatus, healthCheckResult.Status);
            mockMessageBus.Verify(m => m.CheckHealthAsync(), Times.Once);
            Assert.IsAssignableFrom<IHealthCheck>(sut);
        }
    }
}
