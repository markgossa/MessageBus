using System.Collections.Generic;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    public interface IMessageTracker
    {
        List<string> Ids { get; set; }
    }
}