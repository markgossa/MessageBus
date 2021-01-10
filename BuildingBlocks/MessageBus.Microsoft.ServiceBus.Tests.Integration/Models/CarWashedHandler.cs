using MessageBus.Abstractions;

namespace EventBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class CarWashedHandler : IHandleMessages<CarWashed>
    {
        public void Handle(CarWashed message) => System.Console.WriteLine("Car is clean");
    }
}
