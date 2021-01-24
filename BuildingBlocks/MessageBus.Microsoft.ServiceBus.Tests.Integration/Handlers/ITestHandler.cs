using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public interface ITestHandler
    {
        Task MessageHandler(MessageReceivedEventArgs args);
        Task ErrorMessageHandler(MessageErrorReceivedEventArgs arg);
    }
}
