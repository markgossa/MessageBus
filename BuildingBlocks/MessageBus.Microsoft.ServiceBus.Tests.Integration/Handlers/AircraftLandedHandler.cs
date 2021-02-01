using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftLandedHandler : IMessageHandler<AircraftLanded>
    {
        public int MessageCount { get; private set; }
        private readonly string _deadLetterReason;

        public AircraftLandedHandler(string deadLetterReason = null)
        {
            _deadLetterReason = deadLetterReason;
        }

        public async Task HandleAsync(IMessageContext<AircraftLanded> context)
        {
            MessageCount++;

            if (string.IsNullOrWhiteSpace(context.Message.FlightNumber))
            {
                await DeadLetterMessageAsync(context);

                return;
            }

            throw new ArgumentException("FlightNumber cannot be empty");
        }

        private async Task DeadLetterMessageAsync(IMessageContext<AircraftLanded> context)
        {
            if (string.IsNullOrWhiteSpace(_deadLetterReason))
            {
                await context.DeadLetterMessageAsync();
            }
            else
            {
                await context.DeadLetterMessageAsync(_deadLetterReason);
            }
        }
    }
}
