using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit.Models
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context) 
            => await Task.Run(() => System.Console.WriteLine("Jeronimo!"));
    }
}
