using MessageBus.Abstractions;
using ServiceBus1.Events;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceBus1.Handlers
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public async Task HandleAsync(MessageContext<AircraftTakenOff> context)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(5));
            Console.WriteLine($"{nameof(AircraftTakenOff)} message received with AircraftId: {context.Message.AircraftId}");

            var rawMessageText = context.Body.ToString();
            Console.WriteLine($"Raw message as text: {rawMessageText}");

            var jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            Console.WriteLine($"AircraftId using JSON serializer options: {context.Body.ToObjectFromJson<AircraftTakenOff>(jsonOptions).AircraftId}");

            Console.WriteLine($"MessageId for message: {context.MessageId}");
        }
    }
}
