﻿using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Threading.Tasks;

namespace MessageBus.Abstractions.Tests.Unit.Handlers
{
    public class AircraftLandedHandler : IHandleMessages<AircraftLanded>
    {
        public string AircraftId { get; private set; }
        public int MessageCount { get; private set; }

        public async Task HandleAsync(AircraftLanded message)
        {
            await Task.Delay(TimeSpan.FromSeconds(4));
            AircraftId = message.AircraftId;
            MessageCount++;
        }
    }
}
