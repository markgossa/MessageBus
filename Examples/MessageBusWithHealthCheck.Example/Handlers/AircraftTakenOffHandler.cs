using MessageBus.Abstractions;
using MessageBusWithHealthCheck.Example.Events;
using MessageBusWithHealthCheck.Example.Services;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBusWithHealthCheck.Example.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        private readonly IDependency _service;

        public AircraftTakenOffHandler(IDependency service)
        {
            _service = service;
        }

        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context)
        {
            Console.WriteLine();
            Console.WriteLine($"{nameof(AircraftTakenOff)} message received");

            // Get message context properties
            Console.WriteLine($"MessageId: {context.MessageId}");
            Console.WriteLine($"CorrelationId: {context.CorrelationId}");
            Console.WriteLine($"DeliveryCount: {context.DeliveryCount}");
            Console.WriteLine($"Raw message as text: {context.Body}");

            // Get message properties
            Console.WriteLine($"MessageType: {context.Properties["MessageType"]}");

            try
            {
                // Deserialize message using default deserializer and return AircraftId
                Console.WriteLine($"AircraftId: {context.Message.AircraftId}");

                // Deserialize message using custom JSON Serializer Options
                var jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                Console.WriteLine($"AircraftId using JSON serializer options: {context.Body.ToObjectFromJson<AircraftTakenOff>(jsonOptions).AircraftId}");
            }
            catch (Exception)
            {
                // Dead letter the received message
                await context.DeadLetterMessageAsync("Invalid message");
                throw;
            }

            // Do stuff
            await _service.WriteLineAfterDelay(context.MessageId);
        }
    }
}
