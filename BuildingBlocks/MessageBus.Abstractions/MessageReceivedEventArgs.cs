﻿using System;
using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class MessageReceivedEventArgs
    {
        public BinaryData Message { get; }
        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public int DeliveryCount { get; set; }

        internal readonly object MessageObject;
        public Dictionary<string, string> MessageProperties = new Dictionary<string, string>();

        public MessageReceivedEventArgs(BinaryData message, object messageObject,
            Dictionary<string, string> messageProperties)
        {
            Message = message;
            MessageObject = messageObject;
            MessageProperties = messageProperties;
        }
    }
}
