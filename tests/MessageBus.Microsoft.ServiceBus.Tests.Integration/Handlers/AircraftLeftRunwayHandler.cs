using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftLeftRunwayHandler : IMessageHandler<AircraftLeftRunway>
    {
        public async Task HandleAsync(IMessageContext<AircraftLeftRunway> context)
        {
            var message = new Message<IEvent>(new AircraftReachedGate { AirlineId = context.Message.RunwayId });
            await context.PublishAsync(message);
        }
    }
}
