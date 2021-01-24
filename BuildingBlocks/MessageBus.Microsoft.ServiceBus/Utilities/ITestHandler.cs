using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Utilities
{
    public interface ITestHandler
    {
        Task MessageHandler(MessageContext args);
        Task ErrorMessageHandler(MessageErrorContext arg);
    }
}
