﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageContext<TMessage> where TMessage : IMessage
    {
        BinaryData Body { get; }
        TMessage Message { get; }
        string MessageId { get; }
        string CorrelationId { get; }
        Dictionary<string, string> Properties { get; }
        int DeliveryCount { get; }

        Task DeadLetterAsync();
    }
}