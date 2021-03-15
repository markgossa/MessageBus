using MessageBus.Abstractions.Tests.Unit.Models.Commands;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageTestsBase
    {
        protected const string _defaultMessageVersionPropertyName = "MessageVersion";

        protected static CreateNewFlightPlan BuildCreateNewFlightPlanCommand()
           => new CreateNewFlightPlan
           {
               Source = "EGKK",
               Destination = "LPPD"
           };

        protected static AircraftLanded BuildAircraftLandedEvent()
            => new AircraftLanded
            {
                AircraftId = Guid.NewGuid().ToString()
            };

        protected static Dictionary<string, string> BuildMessageProperties()
            => new Dictionary<string, string>
            {
                            { "MessageVersion", "1" },
                            { "MessageType", nameof(AircraftLanded) },
                            { "AircraftType", "Commercial" }
            };
    }
}
