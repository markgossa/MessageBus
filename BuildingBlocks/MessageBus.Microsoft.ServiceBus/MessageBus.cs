using MessageBus.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventBus.Microsoft.ServiceBus
{
    public class MessageBus
    {
        private readonly string _connectionString;
        private readonly string _topic;
        private readonly string _subscription;
        private readonly string _messageTypePropertyName;

        public MessageBus(string connectionString, string topic, string subscription, 
            string messageTypePropertyName = "MessageType")
        {
            _connectionString = connectionString;
            _topic = topic;
            _subscription = subscription;
            _messageTypePropertyName = messageTypePropertyName;
        }

        public async Task InitializeAsync(ServiceCollection services)
        {
            var client = new SubscriptionClient(_connectionString, _topic, _subscription);
            await RemoveAllRulesAsync(client);

            foreach (var handler in FindRegisteredHandlers(services))
            {
                await AddRulesAsync(client, GetMessageTypeFromHandler(handler.ImplementationType));
            }
        }

        private static IEnumerable<ServiceDescriptor> FindRegisteredHandlers(ServiceCollection services)
            => services.Where(s => s.ServiceType.Name.Contains(typeof(IHandleMessages<>).Name));

        private static async Task RemoveAllRulesAsync(SubscriptionClient client)
        {
            var rules = await client.GetRulesAsync();
            foreach (var rule in rules)
            {
                await client.RemoveRuleAsync(rule.Name);
            }
        }

        private static Type GetMessageTypeFromHandler(Type handler) 
            => handler
                .GetInterfaces()
                .First(i => i.Name.Contains(typeof(IHandleMessages<>).Name))
                .GenericTypeArguments.First();

        private async Task AddRulesAsync(SubscriptionClient client, Type messageType)
            => await client.AddRuleAsync(messageType.Name, new SqlFilter($"{_messageTypePropertyName} = '{messageType.Name}'"));
    }
}
