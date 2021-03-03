using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.LocalMessageBus.Server.Services
{
    public class Topic
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();

        public void AddSubscription(Subscription subscription) => _subscriptions.Add(subscription);
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

        public List<Subscription> GetSubscriptions() => _subscriptions;

        public void UpdateSubsription(Subscription subscription, string name)
        {
            var subscriptionToUpdate = _subscriptions.FirstOrDefault(s => s.Name == name)
                ?? ThrowIfSubscriptionNotFound(name);

            subscriptionToUpdate.Label = subscription.Label;
            subscriptionToUpdate.MessageProperties = subscription.MessageProperties;
        }

        private static Subscription ThrowIfSubscriptionNotFound(string name)
            => throw new InvalidOperationException($"Subscription {name} not found");
    }
}
