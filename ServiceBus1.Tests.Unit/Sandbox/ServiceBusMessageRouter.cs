using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServiceBus1.Events;
using ServiceBus1.Handlers;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceBus1.Tests.Unit
{
    internal class ServiceBusMessageRouter
    {
        private const string topic = "topic1";
        private const string subscription = "ServiceBus1";
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IDefaultMessageHandler _messageProcessor;
        private readonly IAircraftLandedService _aircraftLandedService;
        private ServiceProvider _serviceProvider;

        public ServiceBusMessageRouter(ServiceBusClient serviceBusClient, 
            IDefaultMessageHandler messageProcessor, IAircraftLandedService aircraftLandedService)
        {
            _serviceBusClient = serviceBusClient;
            _messageProcessor = messageProcessor;
            _aircraftLandedService = aircraftLandedService;
        }

        public async Task InitializeAsync()
        {
            var processor = _serviceBusClient.CreateProcessor(topic, subscription);
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += MessageErrorHandler;
            await processor.StartProcessingAsync();
            _serviceProvider = ConfigureServices();
        }

        private static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddSingleton<IHandleMessages<AircraftTakenOff>, AircraftTakenOffHandler>()
                .BuildServiceProvider();

        private Task MessageHandler(ProcessMessageEventArgs arg)
        {
            var messageType = arg.Message.ApplicationProperties["MessageType"].ToString();

            if (messageType == "AircraftLanded")
            {
                _aircraftLandedService.Process(arg.Message.Body.ToString());
            }
            else if (messageType == "AircraftTakenOff")
            {
                var messageTypeType = Assembly.GetExecutingAssembly().GetTypes().First(t => t.Name == messageType);
                var handlerType = typeof(IHandleMessages<>).MakeGenericType(messageTypeType);
                dynamic handler1 = _serviceProvider.GetRequiredService(handlerType);
                var message = JsonSerializer.Deserialize(arg.Message.Body.ToString(), messageTypeType);
                handler1.GetType().GetMethod("Handle").Invoke(handler1, new object[] { message });
            }
            else
            {
                _messageProcessor.Process(arg.Message.Body.ToString());
            }

            return Task.CompletedTask;
        }

        private Task MessageErrorHandler(ProcessErrorEventArgs arg)
                => throw new NotImplementedException();
    }
}