using MessageBus.Abstractions.Messages;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    internal interface ISendingService
    {
        Task SendAsync(ICommand message);
    }
}
