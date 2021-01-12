﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public interface IMessageBusAdminClient
    {
        Task Configure(List<Type> messageHandlers);
    }
}