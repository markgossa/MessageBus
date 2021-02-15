using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class CreateNewFlightPlanHandler : IMessageHandler<CreateNewFlightPlan>
    {
        public async Task HandleAsync(IMessageContext<CreateNewFlightPlan> context)
        {
            var startEnginesCommand = new StartEngines { EngineId = context.Message.Destination };

            await context.SendAsync(new Message<ICommand>(startEnginesCommand));
        }
    }
}
