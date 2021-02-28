using Azure.Messaging.ServiceBus.Administration;
using System;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit
{
    public class AzureServiceBusAdminClientTests : AzureServiceBusAdminClientTestsBase
    {
        [Fact]
        public void ConstructorThrowsIfRequiresSessionIsTrue()
        {
            var createSubscriptionOptions = new CreateSubscriptionOptions(_topic, _subscription)
            {
                LockDuration = TimeSpan.FromSeconds(60),
                MaxDeliveryCount = 5,
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(300),
                RequiresSession = true
            };

            Assert.Throws<InvalidOperationException>(() => new AzureServiceBusAdminClient(_hostname,
                _tenantId, createSubscriptionOptions));
            
            Assert.Throws<InvalidOperationException>(() => new AzureServiceBusAdminClient(_connectionString, 
                createSubscriptionOptions));
        }
    }
}
