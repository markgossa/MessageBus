using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit.Models
{
    public class AircraftLandedHandler : IMessageHandler<AircraftLanded>
    {
        public async Task HandleAsync(MessageContext<AircraftLanded> context) 
            => await Task.Run(() => System.Console.WriteLine("Welcome!"));
    }
}
