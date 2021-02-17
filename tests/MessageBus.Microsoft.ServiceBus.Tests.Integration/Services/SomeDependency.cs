using System.Collections.Generic;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    public class SomeDependency : ISomeDependency
    {
        public List<string> Ids { get; set; }

        public SomeDependency()
        {
            Ids = new List<string>();
        }
    }
}
