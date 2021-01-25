using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit.Models
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public async Task HandleAsync(MessageContext<AircraftTakenOff> context) 
            => await Task.Run(() => System.Console.WriteLine("Jeronimo!"));
    }
}
