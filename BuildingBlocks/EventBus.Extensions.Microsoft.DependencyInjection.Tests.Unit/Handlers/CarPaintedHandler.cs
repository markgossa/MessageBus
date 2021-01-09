using EventBus.Abstractions;
using EventBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;

namespace EventBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers
{
    public class CarPaintedHandler : IHandleMessages<CarPainted>
    {
        public void Handle(CarPainted message) => System.Console.WriteLine("Car is clean");
    }
}
