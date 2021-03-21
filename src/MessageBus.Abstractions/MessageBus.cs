using MessageBus.Abstractions.Messages;
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
            await _messageBusAdminClient.ConfigureAsync(_messageHandlerResolver.GetMessageHandlerMappings(),
                _messageBusOptions);
        }

        public async Task DeadLetterMessageAsync(object message, string? reason = null) 
            => await _messageBusClient.DeadLetterMessageAsync(message, reason);

        public async Task<bool> CheckHealthAsync() => await _messageBusAdminClient.CheckHealthAsync();

        public Task StopAsync() => _messageBusClient.StopAsync();

        public IMessageBus SubscribeToMessage<TMessage, TMessageHandler>(SubscriptionFilter? subscriptionFilter = null)
            where TMessage : IMessage
            where TMessageHandler : IMessageHandler<TMessage>
        {
            subscriptionFilter = BuildSubscriptionFilter<TMessage>(subscriptionFilter);
            _messageHandlerResolver.SubcribeToMessage<TMessage, TMessageHandler>(subscriptionFilter);

            return this;
        }

        public async Task PublishAsync(Message<IEvent> eventObject) => await _messageBusClient.PublishAsync(eventObject);

        public async Task SendAsync(Message<ICommand> command) => await _messageBusClient.SendAsync(command);

        public IMessageBus AddMessagePreProcessor<T>() where T : class, IMessagePreProcessor
        {
            _messageProcessorResolver.AddMessagePreProcessor<T>();

            return this;
        }

        public IMessageBus AddMessagePostProcessor<T>() where T : class, IMessagePostProcessor
        {
            _messageProcessorResolver.AddMessagePostProcessor<T>();

            return this;
        }

        public async Task SendMessageCopyAsync(object messageObject, int delayInSeconds = 0) 
            => await _messageBusClient.SendMessageCopyAsync(messageObject, delayInSeconds);

        public async Task SendMessageCopyAsync(object messageObject, DateTimeOffset enqueueTime)
            => await _messageBusClient.SendMessageCopyAsync(messageObject, enqueueTime);

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
            _messageHandlerResolver.Resolve(args.Label ?? args.MessageProperties[_messageTypePropertyName])
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

        private string GetMessageType<TMessage>(SubscriptionFilter? subscriptionFilter) where TMessage : IMessage
        {
            string? messageTypeProperty = null;
            subscriptionFilter?.MessageProperties.TryGetValue(_messageBusOptions.MessageTypePropertyName, out messageTypeProperty);
            
            return subscriptionFilter?.Label
                    ?? messageTypeProperty
                    ?? typeof(TMessage).Name;
        }

        private SubscriptionFilter BuildSubscriptionFilter<TMessage>(SubscriptionFilter? subscriptionFilter) where TMessage : IMessage
        {
            subscriptionFilter ??= new SubscriptionFilter();

            subscriptionFilter.Build(_messageBusOptions, typeof(TMessage));
            return subscriptionFilter!;
        }

        private static string? GetMessageVersion<T>(Message<T> message) where T : IMessage
            => message.Body?.GetType().CustomAttributes.FirstOrDefault(b =>
                b.AttributeType == typeof(MessageVersionAttribute))?.ConstructorArguments.FirstOrDefault().Value?.ToString();
    }
}
