# MessageBus

## Overview

Abstraction layer for messaging technologies such as Azure Service Bus and RabbitMQ.

## Getting Started

MessageBus calls the message handler for the message type that is received based on the `MessageType` property on the message. For example, when a message is received with a `MessageType` property set to `AircraftTakenOff`, the `HandleAsync()` method on the handler is called.

### Create a message

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

### Create a message handler

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

#### Deserialize messages

```csharp
// Deserialize message using default deserializer and return AircraftId
Console.WriteLine($"AircraftId: {context.Message.AircraftId}");

// Deserialize message using custom JSON Serializer options
var jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
Console.WriteLine($"AircraftId: {context.Body.ToObjectFromJson<AircraftTakenOff>(jsonOptions).AircraftId}");
```

#### Get message properties

```csharp
Console.WriteLine($"MessageType: {context.Properties["MessageType"]}");
```

#### Get message context properties

```csharp
Console.WriteLine($"MessageId: {context.MessageId}");
Console.WriteLine($"CorrelationId: {context.CorrelationId}");
Console.WriteLine($"DeliveryCount: {context.DeliveryCount}");
Console.WriteLine($"Raw message as text: {context.Body}");
```

### Subscribe to messages and add MessageBus

When starting up, MessageBus will generate a list of the different message types and message handlers then create the subscription and the subscription filters (correlation filters for performance) if they do not already exist. This is idempotent so that redeployments or rolling deployments do not cause downtime.

This can be done using the `IServiceCollection` extension methods in conjunction with .NET Core/.NET 5 Dependency Injection as below:

```csharp
private static ServiceProvider ConfigureServices()
{
    return new ServiceCollection()
        .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
        .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
            Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
            Configuration["ServiceBus:TenantId"]))
        .BuildServiceProvider();
}
```

### Start listening for messages

To start listening for messages, start and configure MessageBus by calling the registered services. 
* `ConfigureAsync()` creates the subscription and subscription filters
* `StartAsync()` starts Message Bus and starts processing messages

```csharp
await serviceProvider.GetRequiredService<IMessageBus>().ConfigureAsync();
await serviceProvider.GetRequiredService<IMessageBus>().StartAsync();
```

## Advanced

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

### Run a hosted service in an ASP.NET app

To do this, call `AddHostedService()` and specify the type argument `MessageBusHostedService`:

```csharp
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus;
using MessageBusWithHealthCheck.Example.Events;
using MessageBusWithHealthCheck.Example.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBusWithHealthCheck.Example
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                    Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                    Configuration["ServiceBus:TenantId"]))
                .AddHostedService<MessageBusHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}
```

### Configuring Service Bus Processor options

You can configure options such as `PrefetchCount` and `MaxConcurrentCalls` by passing `ServiceBusProcessorOptions` as a parameter when building the `AzureServiceBusClient`:

```csharp
private static ServiceProvider ConfigureServices()
{
    var serviceBusAdminClient = new AzureServiceBusAdminClient(Configuration["ServiceBus:Hostname"],
            Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
            Configuration["ServiceBus:TenantId"]);

    var options = new ServiceBusProcessorOptions
    {
        PrefetchCount = 10,
        MaxConcurrentCalls = 10
    };

    var serviceBusClient = new AzureServiceBusClient(Configuration["ServiceBus:Hostname"],
            Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
            Configuration["ServiceBus:TenantId"], options);

    return new ServiceCollection()
        .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
        .AddMessageBus(serviceBusAdminClient, serviceBusClient)
        .BuildServiceProvider();
}
```

### Change the MessageType property name

If using Azure Service Bus, the `AzureServiceBusAdminClient` is used to create and configure the subscription. By default, the message property that determines the message type is called `MessageType` however this can be configured by using the `AzureServiceBusAdminClient` constructor which takes `AzureServiceBusAdminClientOptions` as an optional parameter.

## Programming model

MessageBus is designed to be extensible so you can use it with any messaging technology. See a UML diagram of the abstractions below:

[](Documentation/MessageBusUmlDiagram.jpg)

| Class | Interface | Methods | Description |
| --- | --- | --- | --- |
| `AzureServiceBusClient` | `IMessageBusClient` | `StartAsync()` | Abstraction for the messaging client e.g. the `AzureServiceBusClient` is an abstraction layer for `ServiceBusClient`. `StartAsync()` starts listening for messages.
| `AzureServiceBusAdminClient` | `IMessageBusAdminClient` | `ConfigureAsync()` | Abstraction for the messaging client responsible for administration (creating and configuring messages subscriptions and subscription filters) e.g. the `AzureServiceBusAdminClient` is an abstraction layer for `ServiceBusAdministrationClient`. The `ConfigureAsync()` method on this class is called to do any initial configuration when `MessageBus` is started.
| `MessageBusHandlerResolver` | `IMessageBusHandlerResolver` | `Resolve()`, `GetMessageHandlers()` | The `Resolve()` method returns the message handler Type for a given message based on the MessageType. The `GetMessageHandlers()` method returns a list of all message handlers registered in the IoC container.
| `MessageContext<T>` | `IMessageContext` | None | The `MessageContext` is a wrapper around a received message.
| `AircraftTakenOffHandler` | `IMessageHandler<IMessage>` | `HandleAsync()` | This is a message handler. When a message is received with a `MessageType` property matching the name of a Type which implements `IMessage` (e.g. `AircraftTakenOff`), `HandleAsync()` is called with the `IMessageContext<AircraftTakenOff>`.
| `MessageBus` | `IMessageBus` | `StartAsync()`, `ConfigureAsync()` | Abstraction for receiving messages and routing them to the correct message handler based on MessageType. `StartAsync()` starts the `IMessageBusClient` starts listening to messages. `ConfigureAsync()` uses the `MessageBusAdminClient` to configure the messaging subsystem. When a messge is received, the message handler is found using the `IMessageBusHandlerResolver` and then the `HandleAsync()` method on the handler is called.