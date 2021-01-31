using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftLandedHandler : IMessageHandler<AircraftLanded>
    {
        public async Task HandleAsync(MessageContext<AircraftLanded> context) 
            => await Task.Run(() => System.Console.WriteLine("Welcome!"));
    }
}
