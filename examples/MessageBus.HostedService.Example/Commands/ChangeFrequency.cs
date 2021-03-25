﻿using MessageBus.Abstractions.Messages;

namespace MessageBus.HostedService.Example.Commands
{
    public class ChangeFrequency : ICommand
    {
        public decimal NewFrequency { get; set; }
    }
}
