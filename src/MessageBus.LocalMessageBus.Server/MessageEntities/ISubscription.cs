using System.Collections.Generic;

namespace MessageBus.LocalMessageBus.Server.MessageEntities
{
    public interface ISubscription : IQueue
    {
        string Name { get; }
        string? Label { get; set; }
        Dictionary<string, string> MessageProperties { get; set; }
    }
}