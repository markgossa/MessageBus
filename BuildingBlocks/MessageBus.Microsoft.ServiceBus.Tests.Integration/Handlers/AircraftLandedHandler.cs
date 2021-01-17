using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public async Task HandleAsync(AircraftLanded message) => System.Console.WriteLine("Car is clean");
    }
}
