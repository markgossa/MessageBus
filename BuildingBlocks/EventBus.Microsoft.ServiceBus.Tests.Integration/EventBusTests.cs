using Microsoft.Azure.ServiceBus;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EventBus.Microsoft.ServiceBus.Tests.Integration
{
    public class EventBusTests
    {
        private const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";

        private const string _topic = "topic1";
        private const string _subscription = "ServiceBus1";

        [Fact]
        public async Task UpdatesSubscriptionRulesAsync()
        {
            var client = new SubscriptionClient(_connectionString, _topic, _subscription);

            var sut = new EventBus(_connectionString, _topic, _subscription);
            await sut.InitializeAsync();

            var rules = await client.GetRulesAsync();

            var sqlFilter = new SqlFilter("MessageType = 'AircraftLanded'");

            Assert.Single(rules);
            Assert.Single(rules.Where(r => r.Filter.Equals(sqlFilter)));
        }
    }
}
