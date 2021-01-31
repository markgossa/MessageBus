using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Threading.Tasks;

namespace MessageBus.Abstractions.Tests.Unit.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        public string AircraftId { get; private set; }
        public int MessageCount { get; private set; }
        public string MessageAsJson { get; private set; }

        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context)
        {
            await Task.Delay(TimeSpan.FromSeconds(4));
            AircraftId = context.Message.AircraftId;
            MessageAsJson = context.Body.ToString();

            MessageCount++;
        }
    }
}
