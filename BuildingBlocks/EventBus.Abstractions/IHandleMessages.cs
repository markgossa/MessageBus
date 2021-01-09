namespace EventBus.Abstractions
{
    public interface IHandleMessages<T>
    {
        void Handle(T message);
    }
}