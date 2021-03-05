using MessageBus.LocalMessageBus.Server.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
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
    public class SubscriptionControllerTests : TestsBase
    {
        [Fact]
        public async Task AddsAndGetsMultipleSubscriptions()
        {
            var request1 = new SubscriptionRequest
            {
                Name = "Sub1",
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "ATO" }
                },
                Label = "MyLabel"
            };

            var request2 = new SubscriptionRequest
            {
                Name = "Sub2",
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageProperty", "Property1" }
                },
                Label = "MyLabel2"
            };

            var httpClient = await ExecuteAddSubscriptionRequestAsync(request1);
            await ExecuteAddSubscriptionRequestAsync(request2, httpClient);

            var subscriptions = await ExecuteGetSubscriptionsRequestAsync(httpClient);
            Assert.Equal(2, subscriptions.Count());
            AssertSubscriptionAddedAsync(request1, subscriptions);
            AssertSubscriptionAddedAsync(request2, subscriptions);
        }

        [Fact]
        public async Task DeletesSubscriptions()
        {
            var request1 = new SubscriptionRequest
            {
                Name = "Sub1",
                MessageProperties = new Dictionary<string, string>(),
                Label = "MyLabel"
            };

            var request2 = new SubscriptionRequest
            {
                Name = "Sub2",
                MessageProperties = new Dictionary<string, string>(),
                Label = "MyLabel2"
            };

            var httpClient = await ExecuteAddSubscriptionRequestAsync(request1);
            await ExecuteAddSubscriptionRequestAsync(request2, httpClient);
            await ExecuteDeleteSubscriptionRequestAsync(request1.Name, httpClient);

            var subscriptions = await ExecuteGetSubscriptionsRequestAsync(httpClient);
            Assert.Single(subscriptions);
        }

        [Fact]
        public async Task ReceivesMessages()
        {
            var messageToSend = BuildMessage();
            var subscriptionRequest = new SubscriptionRequest
            {
                Name = "Sub1"
            };

            var httpClient = await ExecuteAddSubscriptionRequestAsync(subscriptionRequest);
            await ExecuteSendMessageRequestAsync(messageToSend, httpClient);
            var receivedMessage = await ExecuteRetrieveMessageRequestAsync(httpClient, subscriptionRequest.Name);

            Assert.Equal(messageToSend.Body, receivedMessage.Body);
            Assert.Equal(messageToSend.Label, receivedMessage.Label);
            Assert.True(IsMatchingDictionary(messageToSend.MessageProperties, receivedMessage.MessageProperties));
        }

        private static async Task<LocalMessage> ExecuteRetrieveMessageRequestAsync(HttpClient httpClient,
            string subscription)
        {
            var client = httpClient ?? new WebApplicationFactory<Startup>().CreateClient();
            var response = await client.GetAsync($"api/subscription/receivemessage?subscription={subscription}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<LocalMessage>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static async Task<HttpClient> ExecuteSendMessageRequestAsync(LocalMessage message,
            HttpClient httpClient = null)
        {
            var client = httpClient ?? new WebApplicationFactory<Startup>().CreateClient();
            await client.PostAsync("api/topic/sendmessage",
                new StringContent(JsonSerializer.Serialize(message),
                Encoding.UTF8, MediaTypeNames.Application.Json));

            return client;
        }
    }
}
