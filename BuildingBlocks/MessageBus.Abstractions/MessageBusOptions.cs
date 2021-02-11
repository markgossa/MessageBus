namespace MessageBus.Abstractions
{
    public class MessageBusOptions
    {
        private const string _defaultMessageTypePropertyName = "MessageType";
        private const string _defaultMessageVersionPropertyName = "MessageVersion";

        public string MessageTypePropertyName { get; set; }
        public string MessageVersionPropertyName { get; set; }
        
        public MessageBusOptions()
        {
            MessageTypePropertyName = _defaultMessageTypePropertyName;
            MessageVersionPropertyName = _defaultMessageVersionPropertyName;
        }
    }
}
