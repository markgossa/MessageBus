using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using MessageBus.LocalMessageBus.Server.Tests.Unit.Events;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class TestsBase
    {

        protected static string BuildPassengerBoardedEventJson(Guid passengerId)
        {
            var passengerBoardedEvent = new PassengerBoarded { PassengerId = passengerId };
            return JsonSerializer.Serialize(passengerBoardedEvent);
        }
        protected static List<LocalMessage> EnqueueMessages(IQueue sut, int count = 1, Guid? passengerId = null)
        {
            var messages = new List<LocalMessage>();
            for (var i = 0; i < count; i++)
            {
                var passengerBoardedEvent = new PassengerBoarded { PassengerId = passengerId ?? Guid.NewGuid() };
                var passengerBoardedEventAsJson = JsonSerializer.Serialize(passengerBoardedEvent);
                var message = new LocalMessage(passengerBoardedEventAsJson);
                messages.Add(message);
                sut.Send(message);
            }

            return messages;
        }
    }
}