using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit.Utilities
{
    public interface ITestHandler
    {
        Task MessageHandler(MessageReceivedEventArgs args);
        Task ErrorMessageHandler(MessageErrorReceivedEventArgs arg);
    }
}
