namespace MessageBus.LocalMessageBus.Server.Models
{
    public class LocalMessage
    {
        public string Body { get; }

        public LocalMessage(string body)
        {
            Body = body;
        }
    }
}
