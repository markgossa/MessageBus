using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MessageBus.LocalMessageBus.Server.Controllers
{
    public class TopicController : Controller
    {
        private readonly ITopic _topic;

        public TopicController(ITopic topic)
        {
            _topic = topic;
        }

        [HttpPost]
        public IActionResult AddSubscription(SubscriptionRequest subscriptionRequest)
        {
            _topic.AddSubscription(new Subscription(new Queue(), subscriptionRequest.Name)
            {
                Label = subscriptionRequest.Label,
                MessageProperties = subscriptionRequest.MessageProperties
            });

            return Accepted();
        }
    }
}
