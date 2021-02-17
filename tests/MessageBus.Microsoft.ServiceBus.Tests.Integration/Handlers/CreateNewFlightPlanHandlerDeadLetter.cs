using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class CreateNewFlightPlanHandlerDeadLetter : IMessageHandler<CreateNewFlightPlan>
    {
        public async Task HandleAsync(IMessageContext<CreateNewFlightPlan> context) 
            => await context.DeadLetterMessageAsync(context.Message.Destination);
    }
}
