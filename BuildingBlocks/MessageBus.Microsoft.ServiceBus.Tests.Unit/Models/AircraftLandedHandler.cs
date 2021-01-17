using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit.Models
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public async Task HandleAsync(AircraftLanded message) => System.Console.WriteLine("Car is clean");
    }
}
