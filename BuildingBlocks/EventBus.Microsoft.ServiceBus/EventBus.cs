using Microsoft.Azure.ServiceBus;
using System.Threading.Tasks;

namespace EventBus.Microsoft.ServiceBus
{
    public class EventBus
    {
        private readonly string _connectionString;
        private readonly string _topic;
        private readonly string _subscription;

        public EventBus(string connectionString, string topic, string subscription)
        {
            _connectionString = connectionString;
            _topic = topic;
            _subscription = subscription;
        }

        public async Task InitializeAsync()
        {
            var client = new SubscriptionClient(_connectionString, _topic, _subscription);
            await RemoveAllRulesAsync(client);
            await AddRulesAsync(client);
        }

        private static async Task RemoveAllRulesAsync(SubscriptionClient client)
        {
            var rules = await client.GetRulesAsync();
            foreach (var rule in rules)
            {
                await client.RemoveRuleAsync(rule.Name);
            }
        }

        private static async Task AddRulesAsync(SubscriptionClient client) 
            => await client.AddRuleAsync("AircraftLanded", new SqlFilter("MessageType = 'AircraftLanded'"));
    }
}
