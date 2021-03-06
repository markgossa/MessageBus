using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        private readonly IMessageTracker _messageTracker;
        public List<string> AircraftIds = new List<string>();

        public AircraftTakenOffHandler(IMessageTracker messageTracker)
        {
            _messageTracker = messageTracker;
        }

        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context)
        {
            _messageTracker.Ids.Add(context.Message.AircraftId);

            var message = new Message<IEvent>(new AircraftLeftAirspace { AircraftIdentifier = context.Message.AircraftId });
            await context.PublishAsync(message);
        }
    }
}
