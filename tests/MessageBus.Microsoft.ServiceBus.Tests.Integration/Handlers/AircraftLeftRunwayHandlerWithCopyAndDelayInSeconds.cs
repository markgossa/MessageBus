using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftLeftRunwayHandlerWithCopyAndDelayInSeconds : IMessageHandler<AircraftLeftRunway>
    {
        private readonly IMessageTracker _messageTracker;

        public AircraftLeftRunwayHandlerWithCopyAndDelayInSeconds(IMessageTracker messageTracker)
        {
            _messageTracker = messageTracker;
        }

        public async Task HandleAsync(IMessageContext<AircraftLeftRunway> context)
        {
            AddMessageIdToMessageTracker(context);
            await PublishAircraftReachedGate(context);

            if (_messageTracker.Ids.Count(i => i == context.MessageId) < 2)
            {
                await context.SendMessageCopyAsync(delayInSeconds: 10);
            }
        }

        private void AddMessageIdToMessageTracker(IMessageContext<AircraftLeftRunway> context) 
            => _messageTracker.Ids.Add(context.MessageId);

        private static async Task PublishAircraftReachedGate(IMessageContext<AircraftLeftRunway> context)
        {
            var message = new Message<IEvent>(new AircraftReachedGate { AirlineId = context.Message.RunwayId });
            await context.PublishAsync(message);
        }
    }
}
