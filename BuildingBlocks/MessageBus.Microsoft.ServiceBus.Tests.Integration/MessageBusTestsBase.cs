using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusTestsBase
    {
        protected const string _connectionString = "Endpoint=sb://sb43719.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=FqCICJRc9BFQbXNaiXDRSmUe1sGLwVpGP1OdcAFdkhQ=;";
        protected const string _topic = "topic1";
        protected const string _subscription = "ServiceBus1";
        protected readonly ServiceBusClient _serviceBusClient = new ServiceBusClient(_connectionString);
        protected readonly ServiceBusAdministrationClient _serviceBusAdminClient = new ServiceBusAdministrationClient(_connectionString);
        protected readonly ServiceBusSender _topicClient;

        public MessageBusTestsBase()
        {
            _topicClient = _serviceBusClient.CreateSender(_topic);
        }

        protected async Task AssertSubscriptionRules(Type[] messageTypes, string messagePropertyName = "MessageType")
        {
            var asyncRules = _serviceBusAdminClient.GetRulesAsync(_topic, _subscription);

            var rules = new List<RuleProperties>();
            await foreach (var rule in asyncRules)
            {
                rules.Add(rule);
            }

            Assert.Equal(messageTypes.Length, rules.Count);
            foreach (var messageType in messageTypes)
            {
                var sqlFilter = new SqlRuleFilter($"{messagePropertyName} = '{nameof(AircraftLanded)}'");
                Assert.Single(rules.Where(r => r.Filter.Equals(sqlFilter)));
                Assert.Single(rules.Where(r => r.Name == nameof(AircraftLanded)));
            }
        }
    }
}