using MessageBus.LocalMessageBus.Server.MessageEntities;
using MessageBus.LocalMessageBus.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MessageBus.LocalMessageBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : Controller
    {
        private readonly ITopic _topic;

        public SubscriptionController(ITopic topic)
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
        public IActionResult RemoveSubscription(string name)
        {
            _topic.RemoveSubscription(name);

            return Ok();
        }

        [HttpGet]
        [Route(nameof(ReceiveMessage))]
        public IActionResult ReceiveMessage(string subscription)
        {
            var message = _topic.GetSubscriptions().FirstOrDefault(n => n.Name == subscription)?.Receive();

            return message is not null
                ? new OkObjectResult(message)
                : NotFound();
        }
    }
}
