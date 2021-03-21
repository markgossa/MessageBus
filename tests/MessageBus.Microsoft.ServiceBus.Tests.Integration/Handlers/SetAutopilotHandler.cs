using MessageBus.Abstractions;
using MessageBus.Abstractions.Messages;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class SetAutopilotHandler : IMessageHandler<SetAutopilot>
    {
        public async Task HandleAsync(IMessageContext<SetAutopilot> context)
        {
            var monitorAutopilotCommand = new MonitorAutopilot { AutopilotIdentifider = context.Message.AutopilotId };

            await context.SendAsync(new Message<ICommand>(monitorAutopilotCommand));
        }
    }
}
