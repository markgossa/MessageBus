using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MessageBus.Abstractions.Tests.Unit")]

namespace MessageBus.Abstractions
{
    public class MessageBusReceiver : IMessageBusReceiver
    {
        private readonly IMessageBusHandlerResolver _messageBusHandlerResolver;
        private readonly IMessageBusAdminClient _messageBusAdminClient;
        private readonly IMessageBusClient _messageBusClient;
        private readonly string _messageTypeProperty;

        public MessageBusReceiver(IMessageBusHandlerResolver messageBusHandlerResolver,
            IMessageBusAdminClient messageBusAdmin, IMessageBusClient messageBusClient, 
            MessageBusReceiverSettings messageBusSettings = null)
        {
            _messageBusHandlerResolver = messageBusHandlerResolver;
            _messageBusAdminClient = messageBusAdmin;
            _messageBusClient = messageBusClient;
            _messageTypeProperty = GetMessageTypeProperty(messageBusSettings);
        }

        public async Task StartAsync()
        {
            await _messageBusClient.StartAsync();
            _messageBusClient.AddMessageHandler(OnMessageReceived);
            _messageBusClient.AddErrorMessageHandler(OnErrorMessageReceived);
        }

        public async Task ConfigureAsync() 
            => await _messageBusAdminClient.ConfigureAsync(_messageBusHandlerResolver.GetMessageHandlers());

        internal async Task OnMessageReceived(MessageReceivedEventArgs args)
        {
            const string handlerHandleMethodName = "HandleAsync";

            var handler = _messageBusHandlerResolver.Resolve(args.MessageProperties[_messageTypeProperty]);
            var handlerTask = handler.GetType().GetMethod(handlerHandleMethodName).Invoke(handler, new object[] { BuildMessageContext(args, handler) });
            await (handlerTask as Task);
        }

        private object BuildMessageContext(MessageReceivedEventArgs args, object handler)
        {
            dynamic messageContext = Activator.CreateInstance(GetMessageContextType(handler), new object[] { args.Message, args.MessageObject, this });
            messageContext.MessageId = args.MessageId;
            messageContext.CorrelationId = args.CorrelationId;
            messageContext.Properties = args.MessageProperties;
            messageContext.DeliveryCount = args.DeliveryCount;

            return messageContext;
        }

        private static Type GetMessageContextType(object handler) 
            => typeof(MessageContext<>).MakeGenericType((Type)GetMessageTypeFromHandler(handler));

        internal async Task OnErrorMessageReceived(MessageErrorReceivedEventArgs args)
            => await Task.Run(() => throw new MessageReceivedException(args.Exception));

        private static string GetMessageTypeProperty(MessageBusReceiverSettings messageBusSettings)
            => string.IsNullOrWhiteSpace(messageBusSettings?.MessageTypeProperty)
                ? "MessageType"
                : messageBusSettings.MessageTypeProperty;

        private static Type GetMessageTypeFromHandler(object handler) 
            => handler.GetType().GetInterfaces()
                .First(i => i.Name.Contains(typeof(IMessageHandler<>).Name))
                .GenericTypeArguments.First();

        public async Task DeadLetterAsync(object message) => await _messageBusClient.DeadLetterAsync(message);
    }
}
