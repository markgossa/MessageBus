using System;

namespace MessageBusWithHealthCheck.Example.Services
{
    public interface IDependency
    {
        void SaveMessageId(Guid messageId);
    }
}