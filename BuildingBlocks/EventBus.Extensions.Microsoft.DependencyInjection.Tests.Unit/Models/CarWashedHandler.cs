using EventBus.Abstractions;

namespace EventBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models
{
    public class CarWashedHandler : IHandleMessages<CarWashed>
    {
        public void Handle(CarWashed message) => System.Console.WriteLine("Car is clean");
    }
}
