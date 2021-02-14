using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        private readonly ISomeDependency _dependency;
        public List<string> AircraftIds = new List<string>();

        public AircraftTakenOffHandler(ISomeDependency dependency)
        {
            _dependency = dependency;
        }

        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context)
        {
            _dependency.Ids.Add(context.Message.AircraftId);
            await Task.Run(() => System.Console.WriteLine("Jeronimo!"));
        }
    }
}
