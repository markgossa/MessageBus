using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class AircraftTakenOffHandler : IHandleMessages<AircraftTakenOff>
    {
        public async Task HandleAsync(MessageContext<AircraftTakenOff> context) 
            => await Task.Run(() => System.Console.WriteLine("Jeronimo!"));
    }
}
