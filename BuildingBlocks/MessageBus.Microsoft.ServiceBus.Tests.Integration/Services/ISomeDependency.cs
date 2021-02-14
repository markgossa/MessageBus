using System.Collections.Generic;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    public interface ISomeDependency
    {
        List<string> Ids { get; set; }
    }
}