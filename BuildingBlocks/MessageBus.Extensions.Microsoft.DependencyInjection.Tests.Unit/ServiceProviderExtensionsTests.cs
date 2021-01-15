using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ServiceBus1.Tests.Unit
{
    public class ServiceProviderExtensionsTests
    {
        [Fact]
        public void SubscribesToMessage()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler));

            var service = services.BuildServiceProvider().GetRequiredService<IHandleMessages<AircraftTakenOff>>();

            Assert.NotNull(service);
            Assert.IsType<AircraftTakenOffHandler>(service);
        }
        
        [Fact]
        public void SubscribesToMessageUsingGenerics()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();

            var service = services.BuildServiceProvider().GetRequiredService<IHandleMessages<AircraftTakenOff>>();

            Assert.NotNull(service);
            Assert.IsType<AircraftTakenOffHandler>(service);
        }
        
        [Fact]
        public void AddMessageBusCreatesAndRegistersIMessageBus()
        {
            var services = CreateServiceCollection();
            var mockMessageBusAdmin = new Mock<IMessageBusAdmin>();
            var mockMessageBusClient = new Mock<IMessageBusClient>();
            var actualServices = services.AddMessageBus(mockMessageBusAdmin.Object, mockMessageBusClient.Object);

            var messageBusService = services.BuildServiceProvider().GetService<IMessageBusService>();

            Assert.NotNull(messageBusService);
            Assert.Equal(services, actualServices);
        }

        private static ServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .SubscribeToMessage<AircraftLanded, AircraftLandedHandler>()
                .AddSingleton<IService1, Service1>();
            return services;
        }
    }
}
