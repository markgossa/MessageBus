using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System.Threading.Tasks;

namespace MessageBus.Abstractions.Tests.Unit.Handlers
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public async Task HandleAsync(AircraftLanded message) => System.Console.WriteLine("Welcome!");
    }
}
