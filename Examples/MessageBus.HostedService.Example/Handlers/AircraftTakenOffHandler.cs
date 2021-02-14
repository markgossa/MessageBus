using MessageBus.Abstractions;
using MessageBus.HostedService.Example.Events;
using MessageBus.HostedService.Example.Services;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBus.HostedService.Example.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        private readonly IDependency _dependency;

        public AircraftTakenOffHandler(IDependency dependency)
        {
            _dependency = dependency;
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
            _dependency.SaveMessageId(Guid.Parse(context.MessageId));
        }
    }
}
