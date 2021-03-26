using System.Collections.Generic;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    public class MessageTracker : IMessageTracker
    {
        public List<string> Ids { get; set; }

        public MessageTracker()
        {
            Ids = new List<string>();
        }
    }
}
