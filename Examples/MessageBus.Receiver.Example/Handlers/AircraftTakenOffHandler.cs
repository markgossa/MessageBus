using MessageBus.Abstractions;
using ServiceBus1.Events;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceBus1.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
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

                // Deserialize message using custom
                var jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                Console.WriteLine($"AircraftId using JSON serializer options: {context.Body.ToObjectFromJson<AircraftTakenOff>(jsonOptions).AircraftId}");
            }
            catch (Exception)
            {
                // Dead letter the received message
                await context.DeadLetterAsync();
                //throw;
            }
        }
    }
}
