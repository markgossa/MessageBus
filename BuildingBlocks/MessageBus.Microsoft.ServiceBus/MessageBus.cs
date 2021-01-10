using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace MessageBus.Microsoft.ServiceBus
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

        public async Task StartAsync(ServiceCollection services)
        {
            var client = new ServiceBusAdministrationClient(_connectionString);
            await RemoveAllRulesAsync(client);

            foreach (var handler in FindRegisteredHandlers(services))
            {
                await AddRulesAsync(client, GetMessageTypeFromHandler(handler.ImplementationType));
            }
        }

        private static IEnumerable<ServiceDescriptor> FindRegisteredHandlers(ServiceCollection services)
            => services.Where(s => s.ServiceType.Name.Contains(typeof(IHandleMessages<>).Name));

        private async Task RemoveAllRulesAsync(ServiceBusAdministrationClient client)
        {
            var rules = client.GetRulesAsync(_topic, _subscription);
            await foreach (var rule in rules)
            {
                await client.DeleteRuleAsync(_topic, _subscription, rule.Name);
            }
        }

        private static Type GetMessageTypeFromHandler(Type handler)
            => handler
                .GetInterfaces()
                .First(i => i.Name.Contains(typeof(IHandleMessages<>).Name))
                .GenericTypeArguments.First();

        private async Task AddRulesAsync(ServiceBusAdministrationClient client, Type messageType)
            => await client.CreateRuleAsync(_topic, _subscription, 
                new CreateRuleOptions(messageType.Name, new SqlRuleFilter($"{_messageTypePropertyName} = '{messageType.Name}'")));
    }
}
