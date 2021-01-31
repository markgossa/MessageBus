# MessageBus

## Overview

Abstraction layer for messaging technologies such as Azure Service Bus and RabbitMQ.

## Programming model

MessageBus is designed to be extensible so you can use it with any messaging technology. See a UML diagram of the abstractions below:

[](Documentation/MessageBusReceiverUmlDiagram.jpg)

| Class | Interface | Methods | Description |
| --- | --- | --- | --- |
| `AzureServiceBusClient` | `IMessageBusClient` | `StartAsync()` | Abstraction for the messaging client e.g. the `AzureServiceBusClient` is an abstraction layer for `ServiceBusClient`. `StartAsync()` starts listening for messages.
| `AzureServiceBusAdminClient` | `IMessageBusAdminClient` | `ConfigureAsync()` | Abstraction for the messaging client responsible for administration (creating and configuring messages subscriptions and subscription filters) e.g. the `AzureServiceBusAdminClient` is an abstraction layer for `ServiceBusAdministrationClient`. The `ConfigureAsync()` method on this class is called to do any initial configuration when `MessageBusReceiver` is started.
| `MessageBusHandlerResolver` | `IMessageBusHandlerResolver` | `Resolve()`, `GetMessageHandlers()` | The `Resolve()` method returns the message handler Type for a given message based on the MessageType. The `GetMessageHandlers()` method returns a list of all message handlers registered in the IoC container.
| `MessageContext<T>` | `IMessageContext` | None | The `MessageContext` is a wrapper around the received message.
| `AircraftTakenOffHandler` | `IMessageHandler<IMessage>` | `HandleAsync()` | This is a message handler. When a message is received with a `MessageType` property matching the name of a Type which implements `IMessage` (e.g. `AircraftTakenOff`), `HandleAsync()` is called with the `IMessageContext<AircraftTakenOff>`.
| `MessageBusReceiver` | `IMessageBusReceiver` | `StartAsync()`, `ConfigureAsync()` | Abstraction for receiving messages and routing them to the correct message handler based on MessageType. `StartAsync()` starts the `IMessageBusClient` starts listening to messages. `ConfigureAsync()` uses the `MessageBusAdminClient` to configure the messaging subsystem. When a messge is received, the message handler is found using the `IMessageBusHandlerResolver` and then the `HandleAsync()` method on the handler is called.