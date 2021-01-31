using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context) 
            => await Task.Run(() => System.Console.WriteLine("Jeronimo!"));
    }
}
