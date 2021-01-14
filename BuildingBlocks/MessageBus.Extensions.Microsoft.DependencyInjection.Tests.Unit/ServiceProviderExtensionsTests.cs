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
        public void AddMessageBusCallsConfigureOnMessageBusAdmin()
        {
            var services = CreateServiceCollection();
            var mockMessageBusAdmin = new Mock<IMessageBusAdmin>();
            var mockMessageBusProcessor = new Mock<IMessageBusProcessor>();
            
            var actualServices = services.AddMessageBus(mockMessageBusAdmin.Object,
                mockMessageBusProcessor.Object);

            var list = new List<Type> { typeof(AircraftTakenOffHandler), typeof(AircraftLandedHandler) };
            mockMessageBusAdmin.Verify(m => m.ConfigureAsync(list), Times.Once);
            Assert.Equal(services, actualServices);
        }

        [Fact]
        public void AddMessageBusCreatesIMessageBusProcessor()
        {
            var services = CreateServiceCollection();
            var mockMessageBusAdmin = new Mock<IMessageBusAdmin>();
            var mockMessageBusProcessor = new Mock<IMessageBusProcessor>();
            services.AddMessageBus(mockMessageBusAdmin.Object, mockMessageBusProcessor.Object);

            var messageBusProcessor = services.BuildServiceProvider().GetService<IMessageBusProcessor>();

            Assert.NotNull(messageBusProcessor);
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
