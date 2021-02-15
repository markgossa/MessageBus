using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftLeftRunwayHandlerDeadLetter : IMessageHandler<AircraftLeftRunway>
    {
        public async Task HandleAsync(IMessageContext<AircraftLeftRunway> context) 
            => await context.DeadLetterMessageAsync(context.Message.RunwayId);
    }
}
