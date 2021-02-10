using System.Collections.Generic;

namespace MessageBus.Abstractions
{
    public class Command : Message
    {
        public ICommand? Message { get; }

        public Command(ICommand command, string? correlationId = null, string? messageId = null,
            Dictionary<string, string>? messageProperties = null) : base(correlationId, 
                messageId, messageProperties)
        {
            Message = command;
        }

        public Command(string commandString) : base(commandString) { }
    }
}
