using MessageBus.LocalMessageBus.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public class Topic : ITopic
    {
        private readonly List<ISubscription> _subscriptions = new List<ISubscription>();

        public void AddSubscription(ISubscription subscription) => _subscriptions.Add(subscription);
        public void RemoveSubscription(string name)
        {
            var subscription = _subscriptions.First(s => s.Name == name)
                ?? ThrowIfSubscriptionNotFound(name);

            _subscriptions.Remove(subscription);
        }

        public void Send(LocalMessage message)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Send(message);
            }
        }

        public List<ISubscription> GetSubscriptions() => _subscriptions;

        public void UpdateSubsription(ISubscription subscription, string name)
        {
            var subscriptionToUpdate = _subscriptions.FirstOrDefault(s => s.Name == name)
                ?? ThrowIfSubscriptionNotFound(name);

            subscriptionToUpdate.Label = subscription.Label;
            subscriptionToUpdate.MessageProperties = subscription.MessageProperties;
        }

        private static ISubscription ThrowIfSubscriptionNotFound(string name)
            => throw new InvalidOperationException($"Subscription {name} not found");
    }
}
