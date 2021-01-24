using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers
{
    public interface ITestHandler
    {
        Task MessageHandler(MessageContext args);
        Task ErrorMessageHandler(MessageErrorContext arg);
    }
}
