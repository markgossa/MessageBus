using MessageBus.Abstractions;
using MessageBus.HostedService.Example.Commands;
using MessageBus.HostedService.Example.Events;
using MessageBus.HostedService.Example.Services;
using System;
using System.Collections.Generic;
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
            Console.WriteLine($"{nameof(AircraftTakenOff)} message received");

            // Get message context properties
            Console.WriteLine($"MessageId: {context.MessageId}");
            Console.WriteLine($"CorrelationId: {context.CorrelationId}");
            Console.WriteLine($"DeliveryCount: {context.DeliveryCount}");
            Console.WriteLine($"Raw message as text: {context.Body}");

            // Get message properties
            foreach (var property in context.Properties)
            {
                Console.WriteLine($"{property.Key}: {property.Value}");
            }

            try
            {
                // Deserialize message using default deserializer and return AircraftId
                Console.WriteLine($"AircraftId: {context.Message.AircraftId}");

                // Deserialize message using custom JSON Serializer Options
                var jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                Console.WriteLine($"AircraftId using JSON serializer options: " +
                    $"{context.Body.ToObjectFromJson<AircraftTakenOff>(jsonOptions).AircraftId}");
            }
            catch (Exception)
            {
                // Dead letter the received message
                await context.DeadLetterMessageAsync("Invalid message");
                throw;
            }

            // Do stuff
            _dependency.SaveMessageId(Guid.Parse(context.MessageId));

            // Publish a new event
            var aircraftLeftAirspaceEvent = new AircraftLeftAirspace { Airspace = "London" };
            await context.PublishAsync(new Message<IEvent>(aircraftLeftAirspaceEvent));

            // Send a command with custom message properties
            var changeFrequency = new ChangeFrequency { NewFrequency = 101.5m };
            var changeFrequencyCommand = new Message<ICommand>(changeFrequency)
            {
                MessageId = $"MyMessageId-{context.Message.AircraftId}",
                CorrelationId = $"MyCorrelationId-{context.Message.AircraftId}",
                MessageProperties = new Dictionary<string, string>
                {
                    { "AircraftId", context.Message.AircraftId }
                }
            };

            await context.SendAsync(changeFrequencyCommand);
            Console.WriteLine();
        }
    }
}
