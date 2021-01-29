using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Threading.Tasks;

namespace MessageBus.Abstractions.Tests.Unit.Handlers
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public string AircraftId { get; private set; }
        public int MessageCount { get; private set; }
        public MessageContext<AircraftLanded> MessageContext { get; private set; }

        public async Task HandleAsync(MessageContext<AircraftLanded> context)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            AircraftId = context.Message.AircraftId;
            MessageCount++;
            MessageContext = context;
        }
    }
}
