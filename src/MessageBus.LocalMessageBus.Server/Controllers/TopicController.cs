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
        [Route(nameof(SendMessage))]
        public IActionResult SendMessage(LocalMessage message)
        {
            _topic.Send(message);

            return Ok();
        }
    }
}
