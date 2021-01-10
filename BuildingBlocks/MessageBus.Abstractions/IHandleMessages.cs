namespace MessageBus.Abstractions
{
    public interface IHandleMessages<T>
    {
        void Handle(T message);
    }
}