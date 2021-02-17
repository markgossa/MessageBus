using MessageBus.Abstractions;

namespace Message.BusHostedService.Example.Commands
{
    public class ChangeFrequency : ICommand
    {
        public decimal NewFrequency { get; set; }
    }
}
