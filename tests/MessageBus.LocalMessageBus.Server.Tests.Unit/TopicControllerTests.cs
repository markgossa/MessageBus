using MessageBus.LocalMessageBus.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.LocalMessageBus.Server.Tests.Unit
{
    public class TopicControllerTests : TestsBase
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
            await ExecuteDeleteSubscriptionRequestAsync(request1.Name, httpClient);

            var subscriptions = await ExecuteGetSubscriptionsRequestAsync(httpClient);
            Assert.Single(subscriptions);
        }
    }
}
