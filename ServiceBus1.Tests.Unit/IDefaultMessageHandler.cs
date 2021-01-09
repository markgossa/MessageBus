namespace ServiceBus1.Tests.Unit
{
    public interface IDefaultMessageHandler
    {
        void Process(string messageBody);
    }
}