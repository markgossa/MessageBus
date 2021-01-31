using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context) 
            => await Task.Run(() => System.Console.WriteLine("Jeronimo!"));
    }
}
