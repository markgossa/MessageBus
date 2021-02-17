using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class AircraftLandedHandler : IMessageHandler<AircraftLanded>
    {
        public async Task HandleAsync(IMessageContext<AircraftLanded> context) 
            => await Task.Run(() => System.Console.WriteLine("Welcome!"));
    }
}
