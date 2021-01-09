namespace ServiceBus1.EventBus
{
    public interface IHandleMessages<T>
    {
        void Handle(T message);
    }
}