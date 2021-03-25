using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusAdminClient
    {
        Task ConfigureAsync(IEnumerable<MessageHandlerMapping> messageSubscriptions, 
            MessageBusOptions messageBusOptions = null);
        Task<bool> CheckHealthAsync();
    }
}