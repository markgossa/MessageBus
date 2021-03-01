using MessageBus.Abstractions;
using System;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit.Events
{
    public class PassengerBoarded : IEvent
    {
        public Guid PassengerId { get; internal set; }
    }
}
