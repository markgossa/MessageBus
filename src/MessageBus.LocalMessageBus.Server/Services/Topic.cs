using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Generic;

namespace MessageBus.LocalMessageBus.Server.Services
{
    public class Topic
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();

        public void AddSubscription(Subscription subscription) => _subscriptions.Add(subscription);
        public void RemoveSubscription(Subscription subscription) => _subscriptions.Remove(subscription);

        public void Send(LocalMessage message)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Send(message);
            }
        }
    }
}
