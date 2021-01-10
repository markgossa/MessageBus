using MessageBus.Abstractions;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class CarPaintedHandler : IHandleMessages<CarPainted>
    {
        public void Handle(CarPainted message) => System.Console.WriteLine("Car is red now");
    }
}
