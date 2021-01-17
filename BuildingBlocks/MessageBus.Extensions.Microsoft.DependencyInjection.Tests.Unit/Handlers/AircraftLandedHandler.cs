using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public async Task HandleAsync(AircraftLanded message) => System.Console.WriteLine("Welcome!");
    }
}
