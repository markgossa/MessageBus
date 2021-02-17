using MessageBus.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageBusHealthCheck : IHealthCheck
    {
        private readonly IMessageBus _messageBus;

        public MessageBusHealthCheck(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthCheckContext,
            CancellationToken cancellationToken = default)
            => await _messageBus.CheckHealthAsync()
                    ? new HealthCheckResult(HealthStatus.Healthy)
                    : new HealthCheckResult(HealthStatus.Unhealthy);
    }
}
