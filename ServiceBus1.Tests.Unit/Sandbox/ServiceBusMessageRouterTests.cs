using Azure.Messaging.ServiceBus;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ServiceBus1.Tests.Unit
{
    public class ServiceBusMessageRouterTests
    {
        private const string connectionString = "";
        private const string topic = "topic1";


        //[Fact]
        //public async Task ReceivesAMessageAsync()
        //{
        //    const string messageType = "string";
        //    var messageBody = Guid.NewGuid().ToString();
        //    var mockMessageProcessor = new Mock<IDefaultMessageHandler>(MockBehavior.Strict);
        //    mockMessageProcessor.Setup(m => m.Process(It.IsAny<string>()));
        //    var mockAircraftLandedService = new Mock<IAircraftLandedService>();
        //    var client = await SendMessage(new ServiceBusClient(connectionString), messageBody, messageType);

        //    var sut = new ServiceBusMessageRouter(client, mockMessageProcessor.Object,
        //        mockAircraftLandedService.Object);
        //    await sut.InitializeAsync();
        //    await Task.Delay(TimeSpan.FromSeconds(1));

        //    mockMessageProcessor.Verify(x => x.Process(messageBody), Times.Once);
        //}

        //[Fact]
        //public async Task ReceivesAnAircraftLandedAsync()
        //{
        //    const string messageType = "AircraftLanded";
        //    var messageBody = Guid.NewGuid().ToString();
        //    var mockMessageProcessor = new Mock<IDefaultMessageHandler>(MockBehavior.Strict);
        //    var mockAircraftLandedService = new Mock<IAircraftLandedService>();
        //    var client = await SendMessage(new ServiceBusClient(connectionString), messageBody, messageType);

        //    var sut = new ServiceBusMessageRouter(client, mockMessageProcessor.Object,
        //        mockAircraftLandedService.Object);
        //    await sut.InitializeAsync();
        //    await Task.Delay(TimeSpan.FromSeconds(1));

        //    mockMessageProcessor.Verify(x => x.Process(messageBody), Times.Never);
        //    mockAircraftLandedService.Verify(x => x.Process(messageBody), Times.Once);
        //}

        //[Fact]
        //public async Task ReceivesAnAircraftTakenOffAsync()
        //{
        //    const string messageType = "AircraftTakenOff";
        //    var aircraftTakenOff = new AircraftTakenOff { AircraftId = Guid.NewGuid().ToString() };
        //    var messageBody = JsonSerializer.Serialize(aircraftTakenOff);
        //    var mockMessageProcessor = new Mock<IDefaultMessageHandler>(MockBehavior.Strict);
        //    var mockAircraftLandedService = new Mock<IAircraftLandedService>();
        //    var mockAircraftTakenOffService = new Mock<IAircraftTakenOffService>();
        //    var mockAircraftTakenOffHandler = new Mock<IHandleMessages<AircraftTakenOff>>();
        //    var client = await SendMessage(new ServiceBusClient(connectionString), messageBody, messageType);

        //    var sut = new ServiceBusMessageRouter(client, mockMessageProcessor.Object,
        //        mockAircraftLandedService.Object);
        //    await sut.InitializeAsync();
        //    await Task.Delay(TimeSpan.FromSeconds(1));

        //    mockMessageProcessor.Verify(x => x.Process(messageBody), Times.Never);
        //    mockAircraftLandedService.Verify(x => x.Process(messageBody), Times.Never);
        //    //mockAircraftTakenOffHandler.Verify(x => x.Handle(aircraftTakenOff), Times.Once);
        //    mockAircraftTakenOffService.Verify(x => x.Process(aircraftTakenOff.AircraftId));
        //}

        private static async Task<ServiceBusClient> SendMessage(ServiceBusClient serviceBusClient,
            string messageBody, string messageType)
        {
            var sender = serviceBusClient.CreateSender(topic);
            var message = new ServiceBusMessage(messageBody);
            message.ApplicationProperties.Add("MessageType", messageType);
            await sender.SendMessageAsync(message);
            return serviceBusClient;
        }
    }
}
