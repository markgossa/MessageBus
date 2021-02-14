using System;

namespace MessageBus.HostedService.Example.Services
{
    public interface IDependency
    {
        void SaveMessageId(Guid messageId);
    }
}