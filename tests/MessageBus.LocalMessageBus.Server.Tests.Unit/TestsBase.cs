using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using MessageBus.LocalMessageBus.Server.Tests.Unit.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

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

        protected static async Task<HttpClient> ExecuteAddSubscriptionRequestAsync(SubscriptionRequest request, 
            HttpClient httpClient = null)
        {
            var client = httpClient ?? new WebApplicationFactory<Startup>().CreateClient();
            var response = await client.PostAsync("api/topic", new StringContent(JsonSerializer.Serialize(request),
                Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();
            
            return client;
        }

        protected static async Task<IEnumerable<Subscription>> ExecuteGetSubscriptionsRequestAsync(HttpClient httpClient)
        {
            var response = await httpClient.GetAsync("api/topic");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<IEnumerable<Subscription>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        protected static void AssertSubscriptionAddedAsync(SubscriptionRequest request, IEnumerable<Subscription> subscriptions) 
            => Assert.Single(subscriptions.Where(s =>
                s.Label == request.Label
                && IsMatchingDictionary(s.MessageProperties, request.MessageProperties)
                && s.Name == request.Name));

        protected static bool IsMatchingDictionary(Dictionary<string, string> dictionary1,
            Dictionary<string, string> dictionary2)
        {
            foreach (var keyValuePair in dictionary1)
            {
                if (!dictionary2.TryGetValue(keyValuePair.Key, out var value)
                    || value != keyValuePair.Value)
                {
                    return false;
                }
            }

            return dictionary1.Count == dictionary2.Count;
        }

        protected static async Task ExecuteDeleteSubscriptionRequestAsync(string name, HttpClient httpClient)
            => await httpClient.DeleteAsync($"api/topic?name={name}");
    }
}