using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class CarWashedHandler : IHandleMessages<CarWashed>
    {
        public void Handle(CarWashed message) => System.Console.WriteLine("Car is clean");
    }
}
