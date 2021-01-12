using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace MessageBus.Microsoft.ServiceBus
{
    public class MessageBus : IMessageBus
    {
        private readonly string _connectionString;
        private readonly string _topic;
        private readonly string _subscription;
        private readonly string _messageTypePropertyName;
        private readonly ServiceBusAdministrationClient _serviceBusAdminClient;
        private IEnumerable<ServiceDescriptor> _handlers;
        private ServiceProvider _serviceProvider;

        public MessageBus(string connectionString, string topic, string subscription,
            string messageTypePropertyName = "MessageType")
        {
            _connectionString = connectionString;
            _topic = topic;
            _subscription = subscription;
            _messageTypePropertyName = messageTypePropertyName;
            _serviceBusAdminClient = new ServiceBusAdministrationClient(_connectionString);
        }

        public async Task StartAsync(ServiceCollection services)
        {
            await RemoveAllRulesAsync(_serviceBusAdminClient);
            await AddRulesAsync(services);

            _serviceProvider = services.BuildServiceProvider();
            await BuildServiceBusProcessor();
        }

        private async Task AddRulesAsync(ServiceCollection services)
        {
            _handlers = FindRegisteredHandlers(services);
            foreach (var handler in _handlers)
            {
                await AddRuleAsync(_serviceBusAdminClient, GetMessageTypeFromHandler(handler.ImplementationType));
            }
        }

        private async Task BuildServiceBusProcessor()
        {
            var options = new ServiceBusProcessorOptions()
            {
                AutoCompleteMessages = true,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
            };

            var serviceBusProcessor = new ServiceBusClient(_connectionString).CreateProcessor(_topic, _subscription, options);
            AddMessageHandlers(serviceBusProcessor);
            await serviceBusProcessor.StartProcessingAsync();
        }

        private void AddMessageHandlers(ServiceBusProcessor serviceBusProcessor)
        {
            serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
            serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            Console.WriteLine($"Some issue: {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var messageType = args.Message.ApplicationProperties["MessageType"].ToString();
            var handlerServiceDescriptor = _handlers.First(h => GetMessageTypeFromHandler(h.ImplementationType).Name == messageType);

            var messageTypeType = GetMessageTypeFromHandler(handlerServiceDescriptor.ImplementationType);
            var message = JsonSerializer.Deserialize(args.Message.Body.ToString(), messageTypeType);

            var handlerInstance = _serviceProvider.GetRequiredService(handlerServiceDescriptor.ServiceType);
            handlerServiceDescriptor.ImplementationType.GetMethod("Handle").Invoke(handlerInstance, new object[] { message });

            return Task.CompletedTask;
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

        private async Task AddRuleAsync(ServiceBusAdministrationClient client, Type messageType)
            => await client.CreateRuleAsync(_topic, _subscription,
                new CreateRuleOptions(messageType.Name, new SqlRuleFilter($"{_messageTypePropertyName} = '{messageType.Name}'")));
    }
}
