using System;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public interface ITestHandler
    {
        Task MessageHandler(EventArgs args);
        Task ErrorMessageHandler(EventArgs arg);
    }
}
