using MessageBus.LocalMessageBus.Server.Models;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public interface IQueue
    {
        void Send(LocalMessage message);
        LocalMessage? Receive();
    }
}
