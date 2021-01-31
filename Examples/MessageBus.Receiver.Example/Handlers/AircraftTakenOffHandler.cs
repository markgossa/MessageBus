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
            await Task.Delay(TimeSpan.FromMilliseconds(5));

            Console.WriteLine();
            Console.WriteLine($"{nameof(AircraftTakenOff)} message received");
            Console.WriteLine($"MessageId: {context.MessageId}");
            Console.WriteLine($"CorrelationId: {context.CorrelationId}");
            Console.WriteLine($"DeliveryCount: {context.DeliveryCount}");
            Console.WriteLine($"AircraftId: {context.Message.AircraftId}");

            Console.WriteLine($"Raw message as text: {context.Body}");

            var jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            Console.WriteLine($"AircraftId using JSON serializer options: {context.Body.ToObjectFromJson<AircraftTakenOff>(jsonOptions).AircraftId}");
        }
    }
}
