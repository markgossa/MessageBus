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

When starting up, MessageBus will generate a list of the different message types and message handlers then create the subscription and the subscription filters if they do not already exist. This is idempotent so that redeployments or rolling deployments do not cause downtime.

This can be done using the `IServiceCollection` extension methods in conjunction with .NET Core/.NET 5 Dependency Injection as below:

```csharp
        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                            Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                            Configuration["ServiceBus:TenantId"]))
                        .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            
            return services.BuildServiceProvider();
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

### Sending messages

You can either publish an IEvent or send an ICommand. In either case, the Type of the message being sent is automatically used as the `MessageType` property on the sent message. In addition, if you have set the `MessageVersion` attribute on the message class that you are sending, this is automatically added as the `MessageVersion` property on the sent message.

When publishing or sending messages, you can either override or add to the default message properties of the sent message by setting the message properties on the `Message<IEvent>` or `Message<ICommand>` that you are sending. This does not add the `MessageType` or `MessageVersion` properties to the message.

You can publish an IEvent or simple string. Likewise, you can either send an ICommand or a simple string.

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
            services.AddHostedService<MessageBusHostedService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                        Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                        Configuration["ServiceBus:TenantId"]))
                    .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            services.AddHealthChecks().AddCheck<MessageBusHealthCheck>("MessageBus");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
```

### Message Versioning

When making breaking changes to messages, you may want to pin message receivers to a particular message version so you don't need to update all receivers when your sender sends a new message version with a breaking change. To do this, you can use the `MessageVersion` attribute on the message and this will be added as a `MessageVersion` property on the message.

```csharp
using MessageBus.Abstractions;
using System;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models.V2
{
    [MessageVersion(2)]
    public class AircraftLanded : IEvent
    {
        public string AircraftId { get; init; }
        public string FlightNumber { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
```

### Add health checks

This is done by using the ASP.NET `AddHealthChecks()` and `AddCheck<T>()`  methods and passing `MessageBusHealthCheck` as a parameter. In the case of Azure Service Bus, the health check will check networking and permissions by attempting to get details about the topic.

In the example below, the health status can be checked using http://myservice/health.

```csharp
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus;
using MessageBusWithHealthCheck.Example.Events;
using MessageBusWithHealthCheck.Example.Handlers;
using Microsoft.AspNetCore.Builder;
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
            services.AddHostedService<MessageBusHostedService>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                        Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                        Configuration["ServiceBus:TenantId"]))
                    .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            services.AddHealthChecks().AddCheck<MessageBusHealthCheck>("MessageBus");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
```

### Change the default property names (MessageType and MessageVersion)

If using Azure Service Bus, the `AzureServiceBusAdminClient` is used to create and configure the subscription. By default, the message property that determines the message type is called `MessageType` and the property that determines the message version is called `MessageVersion` however these can be configured by passing `MessageBusOptions` into `AddMessageBus()`.

```csharp
using MessageBus.Abstractions;
using MessageBus.Example.Events;
using MessageBus.Example.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.Example
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public static ServiceProvider Initialize()
        {
            BuildConfiguration();
            return ConfigureServices();
        }

        private static void BuildConfiguration()
            => Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();

        private static ServiceProvider ConfigureServices()
        {
            var options = new MessageBusOptions
            {
                MessageTypePropertyName = "MyMessageType",
                MessageVersionPropertyName = "MyMessageVersion"
            };

            var services = new ServiceCollection();
            services.AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                            Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                            Configuration["ServiceBus:TenantId"]))
                            Configuration["ServiceBus:TenantId"]), options)
                        .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            
            return services.BuildServiceProvider();
        }
    }
}
```

### Using custom message properties for subscription filters

To do this, simply create a `Dictionary<string, string>` to hold the custom message subscription properties and then pass this to the `SubscribeToMessage()` method. Note that specifying custom message properties will mean that the defaults of `MessageType` and `MessageVersion` will not be added so you will need to add these yourself.

```csharp
private static ServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    var customMessageSubscriptionProperties = new Dictionary<string, string>
    {
        { "AircraftType", "Commercial" },
        { "MessageType", nameof(AircraftTakenOff) },
        { "MessageVersion", "1" }
    };

    services.AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                    Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                    Configuration["ServiceBus:TenantId"]))
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>(customMessageSubscriptionProperties);
    
    return services.BuildServiceProvider();
}
```

## Azure Service Bus

To get started with integrating with Azure Service Bus, use the `AzureServiceBusClientBuilder` which will be compatible with either connection string authentication or Managed Identity.

### Getting started (Managed Identity)

```csharp
        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                            Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                            Configuration["ServiceBus:TenantId"]))
                        .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            
            return services.BuildServiceProvider();
        }
```

### Getting started (Connection String)

```csharp
        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:ConnectionString"],
                            Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"]))
                        .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            
            return services.BuildServiceProvider();
        }
```

### Configuring Service Bus Processor options

You can configure options such as `PrefetchCount` and `MaxConcurrentCalls` by using another override on the `AddMessageBus()` method which takes an `AzureServiceBusAdminClient` and `AzureServiceBusClient`. You then pass `ServiceBusProcessorOptions` as a parameter when building the `AzureServiceBusClient`:

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