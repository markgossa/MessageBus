# MessageBus

## Overview

Abstraction layer for messaging technologies such as Azure Service Bus and RabbitMQ.

## Initial configuration

### Subscription configuration

When starting up, MessageBus will generate a list of the different message types and message handlers then create the subscription and the subscription filters (correlation filters for performance) if they do not already exist. This is idempotent so that redeployments or rolling deployments do not cause downtime.

If using Azure Service Bus, the `AzureServiceBusAdminClient` is used to create and configure the subscription. By default, the message property that determines the message type is called `MessageType` however this can be configured by using the `AzureServiceBusAdminClient` constructor which takes `AzureServiceBusAdminClientOptions` as an optional parameter.

## Handling messages

MessageBus calls the message handler for the message type that is received based on the `MessageType` property on the message. For example, when a message is received with a `MessageType` property set to `AircraftTakenOff`, the `HandleAsync()` method on the handler is called.

### Create messages

To create a message, you can either implement the `IEvent` or `ICommand` interface depending on whether you are handling an event or command. An example event is below:

```csharp
using MessageBus.Abstractions;

namespace ServiceBus1.Events
{
    public class AircraftTakenOff : IEvent
    {
        public string AircraftId { get; set; }
        public string SourceAirport { get; set; }
    }
}
```

### Message handlers

To create a message handler that receives an `AircraftTakenOff` event, create a class that implements the `IMessageHandler<T>` interface. An simple example is below:

```csharp
using MessageBus.Abstractions;
using ServiceBus1.Events;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceBus1.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context)
        {
            Console.WriteLine($"{nameof(AircraftTakenOff)} message received");
        }
    }
}
```

### Dead lettering messages

Within a message handler, you can call the `DeadLetterAsync()` method on the `IMessageContext` as below. The dead letter reason is optional.

```csharp
using MessageBus.Abstractions;
using ServiceBus1.Events;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceBus1.Handlers
{
    public class AircraftTakenOffHandler : IMessageHandler<AircraftTakenOff>
    {
        public async Task HandleAsync(IMessageContext<AircraftTakenOff> context)
        {
            try
            {
                Console.WriteLine($"AircraftId using JSON serializer options: {context.Body.ToObjectFromJson<AircraftTakenOff>().AircraftId}");
            }
            catch (Exception)
            {
                await context.DeadLetterMessageAsync("Invalid message");
                throw;
            }
        }
    }
}
```

## Programming model

MessageBus is designed to be extensible so you can use it with any messaging technology. See a UML diagram of the abstractions below:

[](Documentation/MessageBusReceiverUmlDiagram.jpg)

| Class | Interface | Methods | Description |
| --- | --- | --- | --- |
| `AzureServiceBusClient` | `IMessageBusClient` | `StartAsync()` | Abstraction for the messaging client e.g. the `AzureServiceBusClient` is an abstraction layer for `ServiceBusClient`. `StartAsync()` starts listening for messages.
| `AzureServiceBusAdminClient` | `IMessageBusAdminClient` | `ConfigureAsync()` | Abstraction for the messaging client responsible for administration (creating and configuring messages subscriptions and subscription filters) e.g. the `AzureServiceBusAdminClient` is an abstraction layer for `ServiceBusAdministrationClient`. The `ConfigureAsync()` method on this class is called to do any initial configuration when `MessageBusReceiver` is started.
| `MessageBusHandlerResolver` | `IMessageBusHandlerResolver` | `Resolve()`, `GetMessageHandlers()` | The `Resolve()` method returns the message handler Type for a given message based on the MessageType. The `GetMessageHandlers()` method returns a list of all message handlers registered in the IoC container.
| `MessageContext<T>` | `IMessageContext` | None | The `MessageContext` is a wrapper around a received message.
| `AircraftTakenOffHandler` | `IMessageHandler<IMessage>` | `HandleAsync()` | This is a message handler. When a message is received with a `MessageType` property matching the name of a Type which implements `IMessage` (e.g. `AircraftTakenOff`), `HandleAsync()` is called with the `IMessageContext<AircraftTakenOff>`.
| `MessageBusReceiver` | `IMessageBusReceiver` | `StartAsync()`, `ConfigureAsync()` | Abstraction for receiving messages and routing them to the correct message handler based on MessageType. `StartAsync()` starts the `IMessageBusClient` starts listening to messages. `ConfigureAsync()` uses the `MessageBusAdminClient` to configure the messaging subsystem. When a messge is received, the message handler is found using the `IMessageBusHandlerResolver` and then the `HandleAsync()` method on the handler is called.