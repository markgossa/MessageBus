﻿using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services
{
    internal class TestPreProcessor2 : IMessagePreProcessor
    {
        public Task ProcessAsync(IMessageContext<IMessage> context) => throw new System.NotImplementedException();
    }
}