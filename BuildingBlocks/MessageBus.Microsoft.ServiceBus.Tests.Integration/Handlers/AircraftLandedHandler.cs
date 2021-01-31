using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftLandedHandler : IMessageHandler<AircraftLanded>
    {
        public int MessageCount { get; private set; }

        public async Task HandleAsync(IMessageContext<AircraftLanded> context)
        {
            MessageCount++;

            if (string.IsNullOrWhiteSpace(context.Message.FlightNumber))
            {
                await context.DeadLetterAsync();
                return;
            }
            
            throw new ArgumentException("FlightNumber cannot be empty");
        }
    }
}
