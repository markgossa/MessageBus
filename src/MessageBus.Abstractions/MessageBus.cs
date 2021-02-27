using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MessageBus.Abstractions.Tests.Unit")]

namespace MessageBus.Abstractions
{
    public class MessageBus : IMessageBus
    {
        private readonly IMessageHandlerResolver _messageHandlerResolver;
        private readonly IMessageBusAdminClient _messageBusAdminClient;
        private readonly IMessageBusClient _messageBusClient;
        private readonly IMessageProcessorResolver _messageProcessorResolver;
        private readonly string _messageTypePropertyName;
        private readonly MessageBusOptions _messageBusOptions;

        public MessageBus(IMessageHandlerResolver messageHandlerResolver,
            IMessageBusAdminClient messageBusAdmin, IMessageBusClient messageBusClient, 
            IMessageProcessorResolver messageProcessorResolver,
            MessageBusOptions? messageBusOptions = null)
        {
            _messageHandlerResolver = messageHandlerResolver;
            _messageBusAdminClient = messageBusAdmin;
            _messageBusClient = messageBusClient;
            _messageProcessorResolver = messageProcessorResolver;
            _messageBusOptions = messageBusOptions ?? new MessageBusOptions();
            _messageTypePropertyName = _messageBusOptions.MessageTypePropertyName;
        }

        public async Task StartAsync()
        {
            await _messageBusClient.StartAsync();
            _messageBusClient.AddMessageHandler(OnMessageReceived);
            _messageBusClient.AddErrorMessageHandler(OnErrorMessageReceived);
        }

        public async Task ConfigureAsync()
        {
            _messageHandlerResolver.Initialize();
            _messageProcessorResolver.Initialize();
            await _messageBusAdminClient.ConfigureAsync(_messageHandlerResolver.GetMessageSubscriptions(),
                _messageBusOptions);
        }

        public async Task DeadLetterMessageAsync(object message, string? reason = null) 
            => await _messageBusClient.DeadLetterMessageAsync(message, reason);

        public async Task<bool> CheckHealthAsync() => await _messageBusAdminClient.CheckHealthAsync();

        public Task StopAsync() => _messageBusClient.StopAsync();

        public IMessageBus SubscribeToMessage<TMessage, TMessageHandler>(Dictionary<string, string>? messageProperties = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>
        {
            _messageHandlerResolver.SubcribeToMessage<TMessage, TMessageHandler>(messageProperties);

            return this;
        }

        public async Task PublishAsync(Message<IEvent> eventObject)
        {
            AddMessageProperties(eventObject);
            await _messageBusClient.PublishAsync(eventObject);
        }

        public async Task SendAsync(Message<ICommand> command)
        {
            AddMessageProperties(command);
            await _messageBusClient.SendAsync(command);
        }

        internal void AddMessagePreProcessor<T>() where T : class, IMessagePreProcessor
            => _messageProcessorResolver.AddMessagePreProcessor<T>();

        internal void AddMessagePostProcessor<T>() where T : class, IMessagePostProcessor
            => _messageProcessorResolver.AddMessagePostProcessor<T>();

        internal async Task OnErrorMessageReceived(MessageErrorReceivedEventArgs args)
            => await Task.Run(() => throw new MessageReceivedException(args.Exception));

        internal async Task OnMessageReceived(MessageReceivedEventArgs args)
        {
            var handler = GetMessageHandler(args);
            var messageType = GetMessageTypeFromHandler(handler);
            var messageContext = BuildMessageContext(args, messageType);

            await CallMessageProcessorsAsync(messageType, messageContext, _messageProcessorResolver.GetMessagePreProcessors());
            await CallMessageHandlerAsync(args, handler, messageContext);
            await CallMessageProcessorsAsync(messageType, messageContext, _messageProcessorResolver.GetMessagePostProcessors());
        }

        private object GetMessageHandler(MessageReceivedEventArgs args) =>
            _messageHandlerResolver.Resolve(args.MessageProperties[_messageTypePropertyName])
                ?? throw new MessageHandlerNotFoundException("Message handler not found or could not be awaited " +
                    $"for MessageId: {args.MessageId}");

        private object BuildMessageContext(MessageReceivedEventArgs args, Type messageTypeType)
        {
            dynamic messageContext = Activator.CreateInstance(BuildMessageContextType(messageTypeType),
                new object[] { args.Message, args.MessageObject, this })
                    ?? throw new ApplicationException($"Unable to build message context for MessageId {args.MessageId}");

            messageContext.MessageId = args.MessageId;
            messageContext.CorrelationId = args.CorrelationId;
            messageContext.Properties = args.MessageProperties;
            messageContext.DeliveryCount = args.DeliveryCount;

            return messageContext;
        }

        private async Task CallMessageProcessorsAsync(Type messageType, object messageContext, 
            IEnumerable<IMessageProcessor> messageProcessors)
        {
            foreach (var processor in messageProcessors)
            {
                var result = processor.GetType().GetMethod("ProcessAsync")?.MakeGenericMethod(messageType)
                    .Invoke(processor, new object[] { messageContext });
                var processorTask = result as Task 
                    ?? throw new TaskSchedulerException($"Could not await ProcessAsync method on {processor.GetType()}");
                await processorTask;
            }
        }
        
        private static async Task CallMessageHandlerAsync(MessageReceivedEventArgs args, object handler, object messageContext)
        {
            const string handlerHandleMethodName = "HandleAsync";
            var handlerResult = handler?.GetType()?.GetMethod(handlerHandleMethodName)?.Invoke(handler, new object[] { messageContext });
            var handlerTask = handlerResult as Task
                ?? throw new MessageHandlerNotFoundException($"Message handler could not be awaited for MessageId: {args.MessageId}");
            await handlerTask;
        }

        private static Type BuildMessageContextType(Type messageTypeType)
            => typeof(MessageContext<>).MakeGenericType(messageTypeType);

        private static Type GetMessageTypeFromHandler(object handler)
            => handler.GetType().GetInterfaces()
                .First(i => i.Name.Contains(typeof(IMessageHandler<>).Name))
                .GenericTypeArguments.First();

        private void AddMessageProperties<T>(Message<T> message) where T : IMessage
        {
            if (!message.OverrideDefaultMessageProperties)
            {
                AddMessageTypeProperty(message);
                AddMessageVersionProperty(message);
            }
        }

        private void AddMessageTypeProperty<T>(Message<T> message) where T : IMessage
        {
            if (message.Body != null)
            {
                message.MessageProperties.Add(_messageBusOptions.MessageTypePropertyName, message.Body.GetType().Name);
            }
        }

        private void AddMessageVersionProperty<T>(Message<T> message) where T : IMessage
        {
            var messageVersion = GetMessageVersion(message);
            if (messageVersion != null)
            {
                message.MessageProperties.Add(_messageBusOptions.MessageVersionPropertyName, messageVersion);
            }
        }

        private static string? GetMessageVersion<T>(Message<T> message) where T : IMessage
            => message.Body?.GetType().CustomAttributes.FirstOrDefault(b =>
                b.AttributeType == typeof(MessageVersionAttribute))?.ConstructorArguments.FirstOrDefault().Value?.ToString();
    }
}
