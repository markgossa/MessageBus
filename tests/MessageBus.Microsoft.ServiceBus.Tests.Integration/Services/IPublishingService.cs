using MessageBus.Abstractions.Messages;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Services
{
    internal interface IPublishingService
    {
        Task PublishAsync(IEvent message);
    }
}
