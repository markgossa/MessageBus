namespace ServiceBus1.Tests.Unit
{
    public interface IAircraftLandedService
    {
        void Process(string messageBody);
    }
}