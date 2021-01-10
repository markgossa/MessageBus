using MessageBus.Abstractions;
using EventBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;

namespace EventBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class CarWashedHandler : IHandleMessages<CarWashed>
    {
        public void Handle(CarWashed message) => System.Console.WriteLine("Car is clean");
    }
}
