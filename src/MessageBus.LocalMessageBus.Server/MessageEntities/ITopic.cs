using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Generic;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public interface ITopic
    {
        void AddSubscription(ISubscription subscription);
        void RemoveSubscription(string name);
        void Send(LocalMessage message);
        List<ISubscription> GetSubscriptions();
        void UpdateSubsription(ISubscription subscription, string name);
    }
}
