using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace MessageBus.LocalMessageBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            _topic.AddSubscription(new Subscription(subscriptionRequest.Name, new Queue())
            {
                Label = subscriptionRequest.Label,
                MessageProperties = subscriptionRequest.MessageProperties
            });

            return Accepted();
        }

        [HttpGet]
        public IActionResult GetSubscriptions() => new OkObjectResult(_topic.GetSubscriptions());

        [HttpDelete]
        public IActionResult DeleteSubscription(string name)
        {
            _topic.RemoveSubscription(name);

            return Ok();
        }
    }
}
